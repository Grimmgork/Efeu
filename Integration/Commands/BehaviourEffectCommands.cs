using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Runtime.Data;
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
        private readonly EfeuEnvironment environment;

        public BehaviourEffectCommands(EfeuEnvironment environment, IBehaviourEffectRepository behaviourEffectRepository, IUnitOfWork unitOfWork)
        {
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.environment = environment;
            this.unitOfWork = unitOfWork;
        }

        public Task CreateEffect(EfeuMessage message, DateTimeOffset timestamp)
        {
            return CreateEffectsBulk([message], timestamp);
        }

        private Task CreateEffectsBulk(EfeuMessage[] messages, DateTimeOffset timestamp)
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

        public BehaviourEffectEntity GetEffectFromMessage(EfeuMessage message, DateTimeOffset timestamp)
        {
            return new BehaviourEffectEntity()
            {
                Id = 0,
                CreationTime = timestamp,
                Name = message.Name,
                TriggerId = message.TriggerId,
                Input = message.Data,
                Tag = environment.EffectProvider.TryGetEffect(message.Name) is null ?
                    BehaviourEffectTag.Signal : BehaviourEffectTag.Effect,
                CorrelationId = message.CorrelationId,
                State = BehaviourEffectState.Running
            };
        }

        public async Task NudgeEffect(int id)
        {
            await behaviourEffectRepository.NudgeAsync(id);
        }

        public async Task SkipEffect(int id, EfeuValue output = default)
        {
            Guid workerId = Guid.NewGuid();
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            if (!await behaviourEffectRepository.TryLockAsync(id, workerId, timestamp, TimeSpan.FromSeconds(30)))
                return;

            BehaviourEffectEntity? effect = await behaviourEffectRepository.GetByIdAsync(id);
            if (effect == null)
                return;

            await unitOfWork.BeginAsync();
            if (effect.TriggerId != Guid.Empty)
            {
                // send response message
                await behaviourEffectRepository.CreateAsync(new BehaviourEffectEntity()
                {
                    CreationTime = DateTime.Now,
                    Name = effect.Name,
                    CorrelationId = effect.CorrelationId,
                    TriggerId = effect.TriggerId,
                    Input = output,
                    Tag = BehaviourEffectTag.Signal,
                    State = BehaviourEffectState.Running
                });
            }

            await behaviourEffectRepository.DeleteAsync(effect.Id);
            await unitOfWork.CommitAsync();

            await behaviourEffectRepository.UnlockAsync(workerId);
        }

        public Task DeleteAsync(int id)
        {
            return behaviourEffectRepository.DeleteAsync(id);
        }
    }
}
