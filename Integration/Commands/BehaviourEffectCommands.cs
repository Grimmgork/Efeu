using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class BehaviourEffectCommands : IBehaviourEffectCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBehaviourEffectRepository behaviourEffectRepository;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;
        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly EfeuEnvironment environment;

        public BehaviourEffectCommands(EfeuEnvironment environment, IUnitOfWork unitOfWork, IBehaviourEffectRepository behaviourEffectRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, IBehaviourTriggerCommands behaviourTriggerCommands, IBehaviourTriggerRepository behaviourTriggerRepository)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
            this.environment = environment;
        }

        public Task CreateEffect(EfeuMessage message, DateTimeOffset timestamp)
        {
            return CreateEffectsBulk([message], timestamp);
        }

        public async Task NudgeEffect(int id)
        {
            await unitOfWork.BeginAsync();

            BehaviourEffectEntity? effect = await behaviourEffectRepository.GetByIdAsync(id);
            if (effect == null)
                return;

            if (effect.State == BehaviourEffectState.Error)
            {
                effect.State = BehaviourEffectState.Running;
                await behaviourEffectRepository.UpdateAsync(effect);
            }

            await unitOfWork.CommitAsync();
        }

        private Task CreateEffectsBulk(EfeuMessage[] messages, DateTimeOffset timestamp)
        {
            List<BehaviourEffectEntity> entities = new List<BehaviourEffectEntity>();
            foreach (EfeuMessage message in messages)
            {
                if (string.IsNullOrWhiteSpace(message.Name))
                    throw new Exception("Message name must not be empty.");

                entities.Add(new BehaviourEffectEntity()
                {
                    CreationTime = timestamp,
                    Name = message.Name,
                    TriggerId = message.TriggerId,
                    Input = message.Data,
                    CorrelationId = message.CorrelationId,
                    State = BehaviourEffectState.Running
                });
            }

            return behaviourEffectRepository.CreateBulkAsync(entities.ToArray());
        }

        public async Task RunEffect(int id)
        {
            await unitOfWork.BeginAsync();

            BehaviourEffectEntity? effect = await behaviourEffectRepository.GetByIdAsync(id);
            if (effect == null)
                return;

            try
            {
                IEffect? effectInstance = environment.EffectProvider.TryGetEffect(effect.Name);
                if (effectInstance is not null)
                {
                    // run effect
                    EffectExecutionContext context = new EffectExecutionContext(effect.Id, effect.CorrelationId, effect.Times, effect.Data);
                    await effectInstance.RunAsync(context, default);

                    if (effect.TriggerId != Guid.Empty)
                    {
                        // send response message
                        await ProcessSignal(new EfeuMessage()
                        {
                            Name = effect.Name,
                            CorrelationId = effect.CorrelationId,
                            TriggerId = effect.TriggerId,
                            Tag = EfeuMessageTag.Response,
                            Data = context.Output
                        });
                    }

                    await behaviourEffectRepository.DeleteAsync(id);
                }
                else
                {
                    // run effect as signal
                    await ProcessSignal(new EfeuMessage()
                    {
                        Name = effect.Name,
                        CorrelationId = effect.CorrelationId,
                        Data = effect.Data,
                        Tag = EfeuMessageTag.Effect,
                        TriggerId = effect.TriggerId,
                    });

                    await behaviourEffectRepository.DeleteAsync(id);
                }
            }
            catch (Exception ex)
            {
                effect.State = BehaviourEffectState.Error;
                effect.Times++;
                await behaviourEffectRepository.UpdateAsync(effect);
            }

            await unitOfWork.CommitAsync();
        }

        public async Task SkipEffect(int id, EfeuValue output = default)
        {
            await unitOfWork.BeginAsync();
            BehaviourEffectEntity? effect = await behaviourEffectRepository.GetByIdAsync(id);
            if (effect == null)
                throw new Exception($"effect with id {id} not found.");

            if (effect.TriggerId != Guid.Empty)
            {
                // send response message
                await ProcessSignal(new EfeuMessage()
                {
                    Name = effect.Name,
                    CorrelationId = effect.CorrelationId,
                    TriggerId = effect.TriggerId,
                    Tag = EfeuMessageTag.Response,
                    Data = output
                });
            }

            await behaviourEffectRepository.DeleteAsync(id);
            await unitOfWork.CommitAsync();
        }

        private async Task ProcessSignal(EfeuMessage message)
        {
            SignalProcessContext context = new SignalProcessContext(behaviourTriggerRepository, behaviourDefinitionRepository, DateTime.Now);

            // process matching triggers for the signal
            BehaviourTrigger[] matchingTriggers = await context.GetMatchingTriggersAsync(message.Name, message.Tag, message.TriggerId);
            ProcessTriggersWithSignal(context, message, matchingTriggers);

            // while new signals have been produced, process them
            int iterations = 1;
            while (context.Signals.Any())
            {
                EfeuMessage signal = context.Signals.Pop();
                matchingTriggers = await context.GetMatchingTriggersAsync(message.Name, message.Tag, message.TriggerId);
                ProcessTriggersWithSignal(context, signal, matchingTriggers);
                iterations++;
                if (iterations > 50)
                    throw new Exception($"infinite loop detected! ({iterations} iterations)");
            }

            await behaviourTriggerCommands.CreateBulkAsync(context.Triggers.ToArray());
            await behaviourTriggerCommands.DeleteBulkAsync(context.DeletedTriggers.ToArray());
            await CreateEffectsBulk(context.Effects.ToArray(), context.Timestamp);
        }

        private void ProcessTriggersWithSignal(SignalProcessContext context, EfeuMessage message, BehaviourTrigger[] matchingTriggers)
        {
            foreach (BehaviourTrigger trigger in matchingTriggers)
            {
                BehaviourRuntime runtime;
                if (trigger.IsStatic)
                {
                    runtime = BehaviourRuntime.RunStaticTrigger(trigger, message, Guid.NewGuid());
                }
                else
                {
                    runtime = BehaviourRuntime.RunTrigger(trigger, message);
                }

                if (runtime.Result == BehaviourRuntimeResult.Skipped) // test if signal matched the trigger
                    continue;

                if (!trigger.IsStatic)
                {
                    context.DeleteTrigger(trigger);
                }

                foreach (BehaviourTrigger trigger1 in runtime.Triggers)
                {
                    context.Triggers.Add(trigger1);
                }

                foreach (EfeuMessage outMessage in runtime.Messages)
                {
                    if (environment.EffectProvider.IsEffect(outMessage.Name))
                    {
                        context.Effects.Add(outMessage);
                    }
                    else
                    {
                        context.Signals.Push(outMessage);
                    }
                }
            }
        }

        public Task DeleteAsync(int id)
        {
            return behaviourEffectRepository.DeleteAsync(id);
        }

        private class SignalProcessContext
        {
            public readonly DateTimeOffset Timestamp;
            public readonly List<BehaviourTrigger> Triggers = new List<BehaviourTrigger>();
            public readonly List<EfeuMessage> Effects = new List<EfeuMessage>();
            public readonly Stack<EfeuMessage> Signals = new Stack<EfeuMessage>();

            private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
            private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;

            public readonly HashSet<Guid> DeletedTriggers = new();

            public SignalProcessContext(IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, DateTimeOffset timestamp)
            {
                this.behaviourDefinitionRepository = behaviourDefinitionRepository;
                this.behaviourTriggerRepository = behaviourTriggerRepository;
                Timestamp = timestamp;
            }

            private readonly Dictionary<int, BehaviourDefinitionVersionEntity> definitionEntityCache = new();

            public async Task<BehaviourTrigger[]> GetMatchingTriggersAsync(string messageName, EfeuMessageTag messageTag, Guid triggerId)
            {
                BehaviourTriggerEntity[] triggerEntities = await behaviourTriggerRepository.GetMatchingAsync(messageName, messageTag, triggerId);

                BehaviourDefinitionVersionEntity[] definitionEntities = await behaviourDefinitionRepository.GetVersionsByIdsAsync(
                    triggerEntities.Select(i => i.DefinitionVersionId)
                        .Where(i => !definitionEntityCache.ContainsKey(i)).ToArray());

                foreach (BehaviourDefinitionVersionEntity definitionVersionEntity in definitionEntities)
                    definitionEntityCache.Add(definitionVersionEntity.Id, definitionVersionEntity);

                List<BehaviourTrigger> result = new();
                foreach (BehaviourTriggerEntity triggerEntity in triggerEntities)
                {
                    if (DeletedTriggers.Contains(triggerEntity.Id))
                        continue;

                    BehaviourTrigger trigger = new BehaviourTrigger()
                    {
                        Id = triggerEntity.Id,
                        CorrelationId = triggerEntity.CorrelationId,
                        MessageName = triggerEntity.MessageName,
                        MessageTag = triggerEntity.MessageTag,
                        Scope = triggerEntity.Scope,
                        Position = triggerEntity.Position,
                        DefinitionId = triggerEntity.DefinitionVersionId,
                        Step = definitionEntityCache[triggerEntity.DefinitionVersionId].GetPosition(triggerEntity.Position)
                    };
                    result.Add(trigger);
                }

                result.AddRange(
                    Triggers.Where(i => 
                        i.MessageName == messageName && 
                        i.MessageTag == messageTag && 
                        (triggerId == Guid.Empty ? true : i.Id == triggerId)));

                return result.ToArray();
            }

            public void DeleteTrigger(BehaviourTrigger trigger)
            {
                Triggers.RemoveAll(item => item.Id == trigger.Id);
                DeletedTriggers.Add(trigger.Id);
            }
        }
    }
}
