using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Integration.Services;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class EffectCommands : IEffectCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IEffectQueries effectQueries;

        private readonly ITriggerCommands triggerCommands;
        private readonly ITriggerQueries triggerQueries;
        private readonly IBehaviourDefinitionQueries behaviourDefinitionQueries;
        private readonly IDeduplicationKeyCommands dedupicationKeyCommands;
        private readonly IEfeuEffectProvider effectProvider;

        public EffectCommands(IEfeuEffectProvider effectProvider, IEffectQueries effectQueries, IEfeuUnitOfWork unitOfWork, ITriggerCommands triggerCommands, ITriggerQueries triggerQueries, IBehaviourDefinitionQueries behaviourDefinitionQueries, IDeduplicationKeyCommands deduplicationKeyCommands)
        {
            this.effectQueries = effectQueries;
            this.effectProvider = effectProvider;
            this.unitOfWork = unitOfWork;
            this.triggerCommands = triggerCommands;
            this.triggerQueries = triggerQueries;
            this.behaviourDefinitionQueries = behaviourDefinitionQueries;
            this.dedupicationKeyCommands = deduplicationKeyCommands;
        }

        public Task CreateEffect(EfeuMessage message)
        {
            return effectQueries.CreateAsync(
                new EffectEntity() {
                    Id = message.Id,
                    Type = message.Type,
                    Tag = message.Tag,
                    Input = message.Data,
                    CorrelationId = message.CorrelationId,
                    CreationTime = message.Timestamp,
                    Data = message.Data,
                    Matter = message.Matter,
                });
        }

        private EffectEntity GetEffectFromOutgoingMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
            if (message.Tag != EfeuMessageTag.Effect)
                throw new Exception("message must be outgoing.");

            return new EffectEntity()
            {
                Id = message.Id,
                CreationTime = timestamp,
                Type = message.Type,
                Input = message.Data,
                State = BehaviourEffectState.Running,
                Matter = message.Matter,
                Tag = effectProvider.TryGetEffect(message.Type) == null ?
                     EfeuMessageTag.Data : EfeuMessageTag.Effect
            };
        }

        public Task NudgeEffect(Guid id)
        {
            return effectQueries.NudgeEffectAsync(id);
        }

        public Task SuspendEffect(Guid id, DateTimeOffset timestamp)
        {
            return effectQueries.SuspendEffectAsync(id, timestamp);
        }

        public Task SkipEffect(Guid id, DateTimeOffset timestamp, EfeuValue output = default)
        {
            return effectQueries.CompleteSuspendedEffectAsync(id, timestamp, output);
        }

        public async Task AbortEffect(Guid id)
        {
            await unitOfWork.BeginAsync();
            await effectQueries.AbortEffectAsync(id);
            await unitOfWork.CompleteAsync();
        }

        public async Task RunImmediate(BehaviourDefinitionStep[] steps, int definitionVersionId, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();

            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), definitionVersionId);

            List<EffectEntity> effects = new List<EffectEntity>();
            foreach (EfeuMessage message in runtime.Messages)
            {
                effects.Add(GetEffectFromOutgoingMessage(message, timestamp));
            }

            await effectQueries.CreateBulkAsync(effects.ToArray());
            await triggerCommands.AttachAsync(runtime.Triggers.ToArray(), timestamp);
            await unitOfWork.CompleteAsync();
        }

        public async Task SendMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");
            if (!await dedupicationKeyCommands.TryInsertAsync(message.Id, timestamp))
            {
                await unitOfWork.CompleteAsync();
                return;
            }

            await SendMessageDeduplicatedAsync(message, timestamp);
            await unitOfWork.CompleteAsync();
        }

        public async Task SendMessageDeduplicatedAsync(EfeuMessage message, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");
            if (message.Tag == EfeuMessageTag.Effect)
            {
                await effectQueries.CreateAsync(new EffectEntity()
                {
                    Id = message.Id,
                    Type = message.Type,
                    Tag = EfeuMessageTag.Effect,
                    CreationTime = timestamp,
                    CorrelationId = message.CorrelationId,
                    Data = message.Data,
                });
            }
            else
            {
                await UnlockTriggers(message, timestamp);
            }

            await unitOfWork.CompleteAsync();
        }

        private async Task UnlockTriggers(EfeuMessage initialSignal, DateTimeOffset timestamp)
        {
            TriggerMatchCache context = new TriggerMatchCache(triggerQueries, behaviourDefinitionQueries, timestamp);
            await context.MatchTriggersAsync(initialSignal);

            int iterations = 0;
            List<EffectEntity> effects = [];
            while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
            {
                EffectEntity effectEntity = GetEffectFromOutgoingMessage(message, context.Timestamp);
                if (effectEntity.Tag == EfeuMessageTag.Effect)
                {
                    effects.Add(effectEntity);
                }
                else
                {
                    iterations++;
                    if (iterations > 50)
                        throw new Exception($"infinite loop detected! ({iterations} iterations)");

                    await context.MatchTriggersAsync(message);
                }
            }

            await triggerCommands.AttachAsync(context.Triggers.ToArray(), context.Timestamp);
            await triggerCommands.DetatchAsync(context.DeletedTriggers.ToArray());
            await triggerCommands.ResolveMattersAsync(context.ResolvedMatters.ToArray());
            await effectQueries.CreateBulkAsync(effects.ToArray());
        }
    }
}
