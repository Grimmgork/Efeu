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
    internal class BehaviourEffectCommands : IBehaviourEffectCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IBehaviourEffectRepository behaviourEffectRepository;

        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;
        private readonly IDeduplicationStore deduplicationStore;
        private readonly IEfeuEffectProvider effectProvider;

        public BehaviourEffectCommands(IEfeuEffectProvider effectProvider, IBehaviourEffectRepository messageRepository, IEfeuUnitOfWork unitOfWork, IBehaviourTriggerCommands behaviourTriggerCommands, IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, IDeduplicationStore deduplicationStore)
        {
            this.behaviourEffectRepository = messageRepository;
            this.effectProvider = effectProvider;
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
            this.deduplicationStore = deduplicationStore;
        }

        public Task CreateEffect(EfeuMessage message)
        {
            return behaviourEffectRepository.CreateAsync(
                new EfeuEffectEntity() {
                    Id = message.Id,
                    Name = message.Name,
                    Tag = message.Tag,
                    Input = message.Data,
                    CorrelationId = message.CorrelationId,
                    TriggerId = message.TriggerId,
                    CreationTime = message.Timestamp,
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
                TriggerId = message.TriggerId,
                Input = message.Data,
                State = BehaviourEffectState.Running,
                Tag = effectProvider.TryGetEffect(message.Name) == null ?
                     EfeuMessageTag.Data : EfeuMessageTag.Effect
            };
        }

        public Task NudgeEffect(Guid id)
        {
            return behaviourEffectRepository.NudgeEffectAsync(id);
        }

        public Task SuspendEffect(Guid id, DateTimeOffset timestamp)
        {
            return behaviourEffectRepository.SuspendEffectAsync(id, timestamp);
        }

        public Task SkipEffect(Guid id, DateTimeOffset timestamp, EfeuValue output = default)
        {
            return behaviourEffectRepository.CompleteSuspendedEffectAsync(id, timestamp, output);
        }

        public Task DeleteEffect(Guid id)
        {
            return behaviourEffectRepository.DeleteEffectAsync(id);
        }

        public async Task RunImmediate(BehaviourDefinitionStep[] steps, int definitionVersionId, DateTimeOffset timestamp)
        {
            unitOfWork.EnsureTransaction();

            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), definitionVersionId);

            List<EfeuEffectEntity> effects = new List<EfeuEffectEntity>();
            foreach (EfeuMessage message in runtime.Messages)
            {
                effects.Add(GetEffectFromOutgoingMessage(message, timestamp));
            }

            await behaviourEffectRepository.CreateBulkAsync(effects.ToArray());
            await behaviourTriggerCommands.AttachAsync(runtime.Triggers.ToArray(), timestamp);
        }

        public async Task SendMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
            unitOfWork.EnsureTransaction();
            await unitOfWork.LockAsync("Trigger");
            if (await deduplicationStore.TryInsertAsync(message.Id.ToString(), message.Timestamp) == 0)
            {
                return;
            }

            if (message.Tag == EfeuMessageTag.Effect)
            {
                await behaviourEffectRepository.CreateAsync(new EfeuEffectEntity()
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
        }

        private async Task UnlockTriggers(EfeuMessage initialSignal, DateTimeOffset timestamp)
        {
            TriggerMatchContext context = new TriggerMatchContext(behaviourTriggerRepository, behaviourDefinitionRepository, timestamp);
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
            await behaviourEffectRepository.CreateBulkAsync(effects.ToArray());
        }
    }
}
