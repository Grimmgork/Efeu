using Antlr4.Build.Tasks;
using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Services
{
    internal class SignalProcessingService : IHostedService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task work = Task.CompletedTask;

        public SignalProcessingService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CancellationToken token = cancellationTokenSource.Token;

            work = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        int execution = await ProcessSignal(token);
                        if (execution == 0)
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource.Cancel();
            return work;
        }

        private async Task<int> ProcessSignal(CancellationToken token)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();
            IBehaviourEffectCommands behaviourEffectCommands = services.GetRequiredService<IBehaviourEffectCommands>();
            IBehaviourTriggerCommands behaviourTriggerCommands = services.GetRequiredService<IBehaviourTriggerCommands>();
            IBehaviourEffectRepository behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectRepository>();
            IBehaviourTriggerRepository behaviourTriggerRepository = services.GetRequiredService<IBehaviourTriggerRepository>();
            IBehaviourDefinitionRepository behaviourDefinitionRepository = services.GetRequiredService<IBehaviourDefinitionRepository>();

            await unitOfWork.BeginAsync();
            await unitOfWork.TryLockAsync("Signal");

            BehaviourEffectEntity? initialEffect = await behaviourEffectRepository.GetRunningSignalAsync();
            if (initialEffect == null)
                return 0;

            try
            {
                EfeuMessage initialMessage = new EfeuMessage()
                {
                    CorrelationId = initialEffect.CorrelationId,
                    Tag = initialEffect.TriggerId == Guid.Empty ? 
                        EfeuMessageTag.Request : EfeuMessageTag.Response,
                    Data = initialEffect.Data,
                    Name = initialEffect.Name,
                    TriggerId = initialEffect.TriggerId
                };

                SignalProcessor context = new SignalProcessor(behaviourTriggerRepository, behaviourDefinitionRepository, DateTime.Now);
                await context.ProcessSignal(initialMessage);

                int iterations = 1;
                List<BehaviourEffectEntity> effects = [];
                while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
                {
                    BehaviourEffectEntity effect = behaviourEffectCommands.GetEffectFromMessage(message, context.Timestamp);
                    if (effect.Tag == BehaviourEffectTag.Effect)
                    {
                        effects.Add(effect);
                    }
                    else
                    {
                        iterations++;
                        if (iterations > 50)
                            throw new Exception($"infinite loop detected! ({iterations} iterations)");

                        await context.ProcessSignal(message);
                    }
                }

                await behaviourTriggerCommands.CreateBulkAsync(context.Triggers.ToArray());
                await behaviourTriggerCommands.DeleteBulkAsync(context.DeletedTriggers.ToArray());
                await behaviourEffectRepository.CreateBulkAsync(effects.ToArray());
                await behaviourEffectCommands.DeleteAsync(initialEffect.Id);
            }
            catch (Exception)
            {
                await behaviourEffectRepository.MarkErrorAsync(initialEffect.Id, initialEffect.Times + 1);
            }

            await unitOfWork.CommitAsync();
            return 1;
        }

        private class SignalProcessor
        {
            public readonly DateTimeOffset Timestamp;
            public readonly List<BehaviourTrigger> Triggers = new List<BehaviourTrigger>();
            public readonly Stack<EfeuMessage> Messages = new Stack<EfeuMessage>();

            private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
            private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;

            public readonly HashSet<Guid> DeletedTriggers = new();

            public SignalProcessor(IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, DateTimeOffset timestamp)
            {
                this.behaviourDefinitionRepository = behaviourDefinitionRepository;
                this.behaviourTriggerRepository = behaviourTriggerRepository;
                Timestamp = timestamp;
            }

            private readonly Dictionary<int, BehaviourDefinitionVersionEntity> definitionEntityCache = new();

            public async Task ProcessSignal(EfeuMessage message)
            {
                BehaviourTrigger[] matchingTriggers = await GetMatchingTriggersAsync(message.Name, message.Tag, message.TriggerId);
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

                    if (runtime.Result == BehaviourRuntimeResult.Skipped)
                        continue;

                    if (!trigger.IsStatic)
                    {
                        Triggers.RemoveAll(item => item.Id == trigger.Id);
                        DeletedTriggers.Add(trigger.Id);
                    }

                    foreach (BehaviourTrigger trigger1 in runtime.Triggers)
                    {
                        Triggers.Add(trigger1);
                    }

                    foreach (EfeuMessage outMessage in runtime.Messages)
                    {
                        Messages.Push(outMessage);
                    }
                }
            }

            private async Task<BehaviourTrigger[]> GetMatchingTriggersAsync(string messageName, EfeuMessageTag messageTag, Guid triggerId)
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
        }
    }
}
