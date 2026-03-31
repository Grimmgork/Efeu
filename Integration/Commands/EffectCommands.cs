using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
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
        private readonly IBehaviourScopeQueries behaviourScopeQueries;
        private readonly IBehaviourScopeCommands behaviourScopeCommands;

        public EffectCommands(IEffectQueries effectQueries, IEfeuUnitOfWork unitOfWork, ITriggerCommands triggerCommands, ITriggerQueries triggerQueries, IBehaviourQueries behaviourQueries, IDeduplicationKeyCommands deduplicationKeyCommands, IBehaviourScopeQueries behaviourScopeQueries, IBehaviourScopeCommands behaviourScopeCommands)
        {
            this.effectQueries = effectQueries;
            this.unitOfWork = unitOfWork;
            this.triggerCommands = triggerCommands;
            this.triggerQueries = triggerQueries;
            this.behaviourQueries = behaviourQueries;
            this.dedupicationKeyCommands = deduplicationKeyCommands;
            this.behaviourScopeQueries = behaviourScopeQueries;
            this.behaviourScopeCommands = behaviourScopeCommands;
        }

        public Task CreateEffect(EfeuMessage message)
        {
            return effectQueries.CreateAsync(message.MapToEffectEntity());
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

        public Task AbortEffect(Guid id)
        {
            return effectQueries.AbortEffectAsync(id);
        }

        public async Task RunImmediate(EfeuBehaviourStep[] steps, int definitionVersionId, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");
            EfeuRuntime runtime = EfeuRuntime.Run(steps, definitionVersionId, timestamp);
            await ProcessMessagesAsync(runtime.Messages.ToArray(), runtime.Triggers.ToArray());
            await unitOfWork.CompleteAsync();
        }

        public async Task SendMessageAsync(EfeuMessage message)
        {
            if (message.Id == Guid.Empty)
            {
                throw new Exception("message id is empty.");
            }

            if (message.Timestamp == DateTimeOffset.MinValue)
            {
                message.Timestamp = DateTime.Now;
            }

            if (message.CorrelationId == Guid.Empty)
            {
                message.CorrelationId = Guid.NewGuid();
            }

            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");

            if (!await dedupicationKeyCommands.TryInsertAsync(message.Id, message.Timestamp))
            {
                await unitOfWork.CompleteAsync();
                return;
            }

            await SendMessageDeduplicatedAsync(message);
            await unitOfWork.CompleteAsync();
        }

        public async Task SendMessageDeduplicatedAsync(EfeuMessage message)
        {
            if (message.Id == Guid.Empty)
            {
                message.Id = Guid.NewGuid();
            }

            if (message.Timestamp == DateTimeOffset.MinValue)
            {
                message.Timestamp = DateTime.Now;
            }

            if (message.CorrelationId == Guid.Empty)
            {
                message.CorrelationId = Guid.NewGuid();
            }

            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");
            if (message.Tag == EfeuMessageTag.Effect)
            {
                await effectQueries.CreateAsync(message.MapToEffectEntity());
            }
            else
            {
                await ProcessMessagesAsync([message], []);
            }

            await unitOfWork.CompleteAsync();
        }

        private async Task ProcessMessagesAsync(EfeuMessage[] messages, EfeuTrigger[] additionalTriggers)
        {
            TriggerEntity[] allTriggerEntities = await triggerQueries.GetAllAsync();
            TriggerProcessingContext context = new TriggerProcessingContext(allTriggerEntities, behaviourQueries, behaviourScopeQueries, additionalTriggers);

            int iterations = 0;
            Stack<EfeuMessage> messageStack = new Stack<EfeuMessage>(messages);
            List<EffectEntity> createdEffects = new List<EffectEntity>();

            while (messageStack.TryPop(out EfeuMessage? message))
            {
                if (message.Tag == EfeuMessageTag.Effect)
                {
                    createdEffects.Add(message.MapToEffectEntity());
                }
                else
                {
                    iterations++;
                    if (iterations > 50)
                        throw new Exception($"infinite loop detected! ({iterations} iterations)");

                    EfeuTrigger[] matchingTriggers = await context.GetMatchingTriggersAsync(message);
                    foreach (EfeuTrigger trigger in matchingTriggers)
                    {
                        EfeuRuntime runtime = EfeuRuntime.RunTrigger(trigger, message);
                        context.Apply(runtime);

                        foreach (EfeuMessage newMessage in runtime.Messages)
                            messageStack.Push(newMessage);
                    }
                }
            }

            BehaviourScopeEntity[] createdScopeEntities = context.CreatedTriggers.MapToBehaviourScopeEntities();

            await triggerCommands.ResolveMattersAsync(context.ResolvedMatters.ToArray());
            await triggerCommands.CompleteGroupsAsync(context.CompletedGroups.ToArray());
            await triggerCommands.CreateBulkAsync(context.CreatedTriggers.ToArray());
            await effectQueries.CreateBulkAsync(createdEffects.ToArray());
            await behaviourScopeCommands.CreateBulkAsync(createdScopeEntities.ToArray());
        }
    }
}
