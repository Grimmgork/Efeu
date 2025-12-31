using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Integration.Services;
using Efeu.Router;
using Efeu.Router.Data;
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

        public Task CreateEffect(EfeuMessage message, DateTimeOffset timestamp)
        {
            return CreateEffectsBulk([message], timestamp);
        }

        public Task CreateEffectsBulk(EfeuMessage[] messages, DateTimeOffset timestamp)
        {
            List<BehaviourEffectEntity> entities = new List<BehaviourEffectEntity>();
            foreach (EfeuMessage message in messages)
            {
                if (string.IsNullOrWhiteSpace(message.Name))
                    throw new Exception("Message name must not be empty.");


                entities.Add(GetEffectFromMessage(message, timestamp));
            }

            return behaviourEffectRepository.CreateBulkAsync(entities.ToArray());
        }

        private BehaviourEffectEntity GetEffectFromMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
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
            return behaviourEffectRepository.DeleteSuspendedEffectAsync(id);
        }

        public async Task ProcessSignal(EfeuMessage initialMessage, int effectId = 0)
        {
            await unitOfWork.Do(async () =>
            {
                await unitOfWork.LockAsync("Trigger");

                SignalProcessingContext context = new SignalProcessingContext(behaviourTriggerRepository, behaviourDefinitionRepository, DateTime.Now);
                await context.ProcessSignalAsync(initialMessage);

                int iterations = 0;
                List<BehaviourEffectEntity> effects = [];
                while (context.Messages.TryPop(out EfeuMessage? message)) // handle produced messages
                {
                    BehaviourEffectEntity effectEntity = GetEffectFromMessage(message, context.Timestamp);
                    if (effectEntity.Tag == BehaviourEffectTag.Incoming)
                    {
                        iterations++;
                        if (iterations > 50)
                            throw new Exception($"infinite loop detected! ({iterations} iterations)");

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
