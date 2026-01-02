using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Integration.Services;
using Efeu.Router;
using Efeu.Router.Data;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly IBehaviourEffectRepository behaviourEffectRepository;

        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;
        private readonly EfeuEnvironment environment;

        public BehaviourEffectCommands(EfeuEnvironment environment, IBehaviourEffectRepository behaviourEffectRepository, IUnitOfWork unitOfWork, IBehaviourTriggerCommands behaviourTriggerCommands, IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository)
        {
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.environment = environment;
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
        }

        public Task CreateEffect(DateTimeOffset timestamp, string name, BehaviourEffectTag tag, EfeuValue input, Guid triggerId, Guid correlationId)
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
            if (message.Tag != EfeuMessageTag.Outgoing)
                throw new Exception("message must be outgoing.");

            return new BehaviourEffectEntity()
            {
                Id = 0,
                CreationTime = timestamp,
                Name = message.Name,
                TriggerId = message.TriggerId,
                Input = message.Data,
                State = BehaviourEffectState.Running,
                Tag = environment.EffectProvider.TryGetEffect(message.Name) == null ?
                     BehaviourEffectTag.Incoming : BehaviourEffectTag.Outgoing
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

        public Task ProcessSignal(EfeuMessage initialMessage, Guid messageId, int effectId = 0)
        {
            return unitOfWork.DoAsync(async () =>
            {
                await unitOfWork.LockAsync("Trigger");

                SignalProcessingContext context = new SignalProcessingContext(behaviourTriggerRepository, behaviourDefinitionRepository, DateTime.Now);
                await context.ProcessSignalAsync(initialMessage);

                int iterations = 0;
                List<BehaviourEffectEntity> effects = [];
                while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
                {
                    BehaviourEffectEntity effectEntity = GetEffectFromOutgoingMessage(message, context.Timestamp);
                    if (effectEntity.Tag == BehaviourEffectTag.Incoming)
                    {
                        iterations++;
                        if (iterations > 50)
                            throw new Exception($"infinite loop detected! ({iterations} iterations)");

                        message.Tag = EfeuMessageTag.Incoming;
                        await context.ProcessSignalAsync(message);
                    }
                    else
                    {
                        effects.Add(effectEntity);
                    }
                }

                await behaviourTriggerCommands.CreateBulkAsync(context.Triggers.ToArray());
                await behaviourTriggerCommands.DeleteBulkAsync(context.DeletedTriggers.ToArray());
                await behaviourEffectRepository.CreateBulkAsync(effects.ToArray());
                if (effectId != 0)
                    await behaviourEffectRepository.DeleteCompletedSignalAsync(effectId);
            });
        }
    }
}
