using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Integration.Services;
using Efeu.Router;
using Efeu.Router.Value;
using Microsoft.Data.SqlClient;
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

        public BehaviourEffectCommands(IEfeuEffectProvider effectProvider, IBehaviourEffectRepository behaviourEffectRepository, IEfeuUnitOfWork unitOfWork, IBehaviourTriggerCommands behaviourTriggerCommands, IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, IDeduplicationStore deduplicationStore)
        {
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.effectProvider = effectProvider;
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
            this.deduplicationStore = deduplicationStore;
        }

        public Task CreateEffect(DateTimeOffset timestamp, string name, EfeuMessageTag tag, EfeuValue input, Guid triggerId, Guid correlationId)
        {
            return behaviourEffectRepository.CreateBulkAsync([new BehaviourEffectEntity() {
                Id = 0,
                Name = name,
                Tag = tag,
                Input = input,
                CorrelationId = correlationId,
                TriggerId = triggerId,
                CreationTime = timestamp,
            }]);
        }

        public Task CreateEffectsBulk(EfeuMessage[] messages, DateTimeOffset timestamp)
        {
            List<BehaviourEffectEntity> entities = new List<BehaviourEffectEntity>();
            foreach (EfeuMessage message in messages)
            {
                entities.Add(GetEffectFromOutgoingMessage(message, timestamp));
            }

            return behaviourEffectRepository.CreateBulkAsync(entities.ToArray());
        }

        public BehaviourEffectEntity GetEffectFromOutgoingMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
            if (message.Tag != EfeuMessageTag.Outbox)
                throw new Exception("message must be outgoing.");

            return new BehaviourEffectEntity()
            {
                Id = 0, // message.Id
                CreationTime = timestamp,
                Name = message.Name,
                TriggerId = message.TriggerId,
                Input = message.Data,
                State = BehaviourEffectState.Running,
                Tag = effectProvider.TryGetEffect(message.Name) == null ?
                     EfeuMessageTag.Signal : EfeuMessageTag.Outbox
            };
        }

        public Task NudgeEffect(int id)
        {
            return behaviourEffectRepository.NudgeEffectAsync(id);
        }

        public Task SuspendEffect(int id, DateTimeOffset timestamp)
        {
            return behaviourEffectRepository.SuspendEffectAsync(id, timestamp);
        }

        public Task SkipEffect(int id, DateTimeOffset timestamp, EfeuValue output = default)
        {
            return behaviourEffectRepository.CompleteSuspendedEffectAsync(id, timestamp, output);
        }

        public Task DeleteEffect(int id)
        {
            return behaviourEffectRepository.DeleteEffectAsync(id);
        }

        public async Task ProcessSignal(EfeuSignal initialSignal, DateTimeOffset timestamp)
        {
            unitOfWork.EnsureTransaction();
            await unitOfWork.LockAsync("Trigger");
            if (await deduplicationStore.TryInsertAsync(initialSignal.Id.ToString(), timestamp) == 0)
            {
                return;
            }

            SignalProcessingContext context = new SignalProcessingContext(behaviourTriggerRepository, behaviourDefinitionRepository, timestamp);
            await context.ProcessSignalAsync(initialSignal);

            int iterations = 0;
            List<BehaviourEffectEntity> effects = [];
            while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
            {
                BehaviourEffectEntity effectEntity = GetEffectFromOutgoingMessage(message, context.Timestamp);
                if (effectEntity.Tag == EfeuMessageTag.Outbox)
                {
                    effects.Add(effectEntity);
                }
                else
                {
                    iterations++;
                    if (iterations > 50)
                        throw new Exception($"infinite loop detected! ({iterations} iterations)");

                    EfeuSignal signal = new EfeuSignal()
                    {
                        Id = Guid.NewGuid(),
                        Name = message.Name,
                        Data = message.Data,
                        Tag = message.Tag,
                        TriggerId = message.TriggerId,
                        Timestamp = timestamp,
                    };
                    await context.ProcessSignalAsync(signal);
                }
            }

            await behaviourTriggerCommands.AttachAsync(context.Triggers.ToArray(), context.Timestamp);
            await behaviourTriggerCommands.DetatchAsync(context.DeletedTriggers.ToArray());
            await behaviourEffectRepository.CreateBulkAsync(effects.ToArray());
        }
    }
}
