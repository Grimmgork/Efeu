using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
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

        public Task CreateEffect(EfeuMessage message)
        {
            return behaviourEffectRepository.CreateAsync(new BehaviourEffectEntity()
            {
                 CreationTime = DateTime.Now,
                 Name = message.Name,
                 TriggerId = message.TriggerId,
                 Data = message.Data,
                 CorrelationId = message.CorrelationId,
                 State = BehaviourEffectState.Running
            });
        }

        public async Task RunEffect(int id)
        {
            BehaviourEffectEntity effect = await behaviourEffectRepository.GetByIdAsync(id);
            if (environment.EffectProvider.IsEffect(effect.Name))
            {
                Console.WriteLine($"Efect: '{effect.Name}'");
            }
            else
            {
                await ProcessSignal(new EfeuMessage()
                {
                    Name = effect.Name,
                    CorrelationId = effect.CorrelationId,
                    Data = effect.Data,
                    Tag = EfeuMessageTag.Effect,
                    TriggerId = effect.TriggerId,
                });
            }
        }

        private async Task ProcessSignal(EfeuMessage message)
        {
            SignalProcessContext context = new SignalProcessContext(behaviourTriggerRepository, behaviourDefinitionRepository);
            await ProcessSignal(context, message);

            foreach (BehaviourTrigger trigger in context.Triggers)
            {
                await behaviourTriggerCommands.CreateAsync(trigger);
            }

            foreach (EfeuMessage effect in context.Effects)
            {
                await CreateEffect(effect);
            }
        }

        private async Task ProcessSignal(SignalProcessContext context, EfeuMessage message)
        {
            BehaviourTrigger[] matchingTriggers = await context.GetMatchingTriggersAsync(message.Name, message.Tag);
            foreach (BehaviourTrigger trigger in matchingTriggers)
            {
                BehaviourRuntime runtime = BehaviourRuntime.RunTrigger(trigger, message);
                if (runtime.Result == BehaviourRuntimeResult.Skipped) // test if signal matched the trigger
                    continue;

                foreach (BehaviourTrigger trigger1 in runtime.Triggers)
                {
                    context.Triggers.Add(trigger1);
                }

                List<EfeuMessage> signals = new();
                foreach (EfeuMessage outMessage in runtime.Messages)
                {
                    if (environment.EffectProvider.IsEffect(outMessage.Name))
                    {
                        context.Effects.Add(outMessage);
                    }
                    else
                    {
                        signals.Add(outMessage);
                    }
                }

                foreach (EfeuMessage signal in signals)
                {
                    await ProcessSignal(context, signal);
                }
            }
        }

        public Task DeleteAsync(int id)
        {
            return behaviourEffectRepository.DeleteAsync(id);
        }

        private class SignalProcessContext
        {
            public readonly List<BehaviourTrigger> Triggers = new List<BehaviourTrigger>();
            public readonly List<EfeuMessage> Effects = new List<EfeuMessage>();

            private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
            private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;

            public SignalProcessContext(IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository)
            {
                this.behaviourDefinitionRepository = behaviourDefinitionRepository;
                this.behaviourTriggerRepository = behaviourTriggerRepository;
            }

            private readonly Dictionary<int, BehaviourDefinitionVersionEntity> definitionEntityCache = new();

            public async Task<BehaviourTrigger[]> GetMatchingTriggersAsync(string messageName, EfeuMessageTag messageTag)
            {
                BehaviourTriggerEntity[] triggerEntities = await behaviourTriggerRepository.GetMatchingAsync(messageName, messageTag);

                BehaviourDefinitionVersionEntity[] definitionEntities = await behaviourDefinitionRepository.GetVersionsByIdsAsync(
                    triggerEntities.Select(i => i.DefinitionVersionId)
                        .Where(i => !definitionEntityCache.ContainsKey(i)).ToArray());

                foreach (BehaviourDefinitionVersionEntity definitionVersionEntity in definitionEntities)
                    definitionEntityCache.Add(definitionVersionEntity.Id, definitionVersionEntity);

                List<BehaviourTrigger> result = new();
                foreach (BehaviourTriggerEntity triggerEntity in triggerEntities)
                {
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
                    Triggers.Where(i => i.MessageName == messageName && i.MessageTag == messageTag));

                return result.ToArray();
            }
        }

    }
}
