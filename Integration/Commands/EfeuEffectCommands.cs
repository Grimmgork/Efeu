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
    internal class EfeuEffectCommands : IEfeuEffectCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IBehaviourEffectQueries behaviourEffectQueries;

        private readonly IEfeuTriggerCommands behaviourTriggerCommands;
        private readonly IEfeuTriggerQueries behaviourTriggerQueries;
        private readonly IBehaviourDefinitionQueries behaviourDefinitionQueries;
        private readonly IDeduplicationKeyCommands dedupicationKeyCommands;
        private readonly IEfeuEffectProvider effectProvider;

        public EfeuEffectCommands(IEfeuEffectProvider effectProvider, IBehaviourEffectQueries behaviourEffectQueries, IEfeuUnitOfWork unitOfWork, IEfeuTriggerCommands behaviourTriggerCommands, IEfeuTriggerQueries behaviourTriggerQueries, IBehaviourDefinitionQueries behaviourDefinitionQueries, IDeduplicationKeyCommands deduplicationKeyCommands)
        {
            this.behaviourEffectQueries = behaviourEffectQueries;
            this.effectProvider = effectProvider;
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerQueries = behaviourTriggerQueries;
            this.behaviourDefinitionQueries = behaviourDefinitionQueries;
            this.dedupicationKeyCommands = deduplicationKeyCommands;
        }

        public Task CreateEffect(EfeuMessage message)
        {
            return behaviourEffectQueries.CreateAsync(
                new EfeuEffectEntity() {
                    Id = message.Id,
                    Name = message.Name,
                    Tag = message.Tag,
                    Input = message.Data,
                    CorrelationId = message.CorrelationId,
                    CreationTime = message.Timestamp,
                    Data = message.Data,
                });
        }

        private EfeuEffectEntity GetEffectFromOutgoingMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
            if (message.Tag != EfeuMessageTag.Effect)
                throw new Exception("message must be outgoing.");

            return new EfeuEffectEntity()
            {
                Id = message.Id,
                CreationTime = timestamp,
                Name = message.Name,
                Input = message.Data,
                State = BehaviourEffectState.Running,
                Tag = effectProvider.TryGetEffect(message.Name) == null ?
                     EfeuMessageTag.Data : EfeuMessageTag.Effect
            };
        }

        public Task NudgeEffect(Guid id)
        {
            return behaviourEffectQueries.NudgeEffectAsync(id);
        }

        public Task SuspendEffect(Guid id, DateTimeOffset timestamp)
        {
            return behaviourEffectQueries.SuspendEffectAsync(id, timestamp);
        }

        public Task SkipEffect(Guid id, DateTimeOffset timestamp, EfeuValue output = default)
        {
            return behaviourEffectQueries.CompleteSuspendedEffectAsync(id, timestamp, output);
        }

        public Task DeleteEffect(Guid id)
        {
            return behaviourEffectQueries.DeleteEffectAsync(id);
        }

        public async Task RunImmediate(BehaviourDefinitionStep[] steps, int definitionVersionId, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();

            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), definitionVersionId);

            List<EfeuEffectEntity> effects = new List<EfeuEffectEntity>();
            foreach (EfeuMessage message in runtime.Messages)
            {
                effects.Add(GetEffectFromOutgoingMessage(message, timestamp));
            }

            await behaviourEffectQueries.CreateBulkAsync(effects.ToArray());
            await behaviourTriggerCommands.AttachAsync(runtime.Triggers.ToArray(), timestamp);
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

            if (message.Tag == EfeuMessageTag.Effect)
            {
                await behaviourEffectQueries.CreateAsync(new EfeuEffectEntity()
                {
                    Id = message.Id,
                    Name = message.Name,
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
            TriggerMatchCache context = new TriggerMatchCache(behaviourTriggerQueries, behaviourDefinitionQueries, timestamp);
            await context.MatchTriggersAsync(initialSignal);

            int iterations = 0;
            List<EfeuEffectEntity> effects = [];
            while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
            {
                EfeuEffectEntity effectEntity = GetEffectFromOutgoingMessage(message, context.Timestamp);
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

            await behaviourTriggerCommands.AttachAsync(context.Triggers.ToArray(), context.Timestamp);
            await behaviourTriggerCommands.DetatchAsync(context.DeletedTriggers.ToArray());
            await behaviourTriggerCommands.ResolveMattersAsync(context.ResolvedMatters.ToArray());
            await behaviourEffectQueries.CreateBulkAsync(effects.ToArray());
        }
    }
}
