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
        private readonly IBehaviourQueries behaviourQueries;
        private readonly IDeduplicationKeyCommands dedupicationKeyCommands;

        public EffectCommands(IEffectQueries effectQueries, IEfeuUnitOfWork unitOfWork, ITriggerCommands triggerCommands, ITriggerQueries triggerQueries, IBehaviourQueries behaviourQueries, IDeduplicationKeyCommands deduplicationKeyCommands)
        {
            this.effectQueries = effectQueries;
            this.unitOfWork = unitOfWork;
            this.triggerCommands = triggerCommands;
            this.triggerQueries = triggerQueries;
            this.behaviourQueries = behaviourQueries;
            this.dedupicationKeyCommands = deduplicationKeyCommands;
        }

        public Task CreateEffect(EfeuMessage message)
        {
            return effectQueries.CreateAsync(
                new EffectEntity() {
                    Id = message.Id,
                    Type = message.Type,
                    Tag = message.Tag,
                    Input = message.Payload,
                    CorrelationId = message.CorrelationId,
                    CreationTime = message.Timestamp,
                    Matter = message.Matter,
                });
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

        public async Task RunImmediate(EfeuBehaviourStep[] steps, int definitionVersionId, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");
            EfeuRuntime runtime = EfeuRuntime.Run(steps, definitionVersionId);
            await UnlockTriggers(runtime.Messages.ToArray(), runtime.Triggers.ToArray(), timestamp);
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
                    Input = message.Payload,
                });
            }
            else
            {
                await UnlockTriggers([message], [], timestamp);
            }

            await unitOfWork.CompleteAsync();
        }

        private async Task UnlockTriggers(EfeuMessage[] signals, EfeuTrigger[] triggers, DateTimeOffset timestamp)
        {
            TriggerMatchCache context = new TriggerMatchCache(triggerQueries, behaviourQueries, timestamp);
            foreach (EfeuMessage signal in signals)
                context.Messages.Push(signal);
            foreach (EfeuTrigger trigger in triggers)
                context.Triggers.Add(trigger);

            int iterations = 0;
            List<EffectEntity> effects = [];
            while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
            {
                if (message.Tag == EfeuMessageTag.Effect)
                {
                    effects.Add(new EffectEntity()
                    {
                        Id = message.Id,
                        Type = message.Type,
                        Tag = message.Tag,
                        Input = message.Payload,
                        CorrelationId = message.CorrelationId,
                        CreationTime = message.Timestamp,
                        Matter = message.Matter,
                    });
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
