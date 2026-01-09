using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class BehaviourTriggerCommands : IBehaviourTriggerCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly IEfeuTriggerProvider triggerProvider;

        public BehaviourTriggerCommands(IEfeuUnitOfWork unitOfWork, IBehaviourTriggerRepository behaviourTriggerRepository, IEfeuTriggerProvider triggerProvider)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.triggerProvider = triggerProvider;
        }

        public Task CreateAsync(BehaviourTrigger trigger, DateTimeOffset timestamp)
        {
            return CreateBulkAsync([trigger], timestamp);
        }

        public Task CreateBulkAsync(BehaviourTrigger[] triggers, DateTimeOffset timestamp)
        {
            List<BehaviourTriggerEntity> entites = new List<BehaviourTriggerEntity>();
            foreach (BehaviourTrigger trigger in triggers)
            {
                entites.Add(new BehaviourTriggerEntity()
                {
                    Id = trigger.Id,
                    DefinitionVersionId = trigger.DefinitionId,
                    CorrelationId = trigger.CorrelationId,
                    Position = trigger.Position,
                    Scope = trigger.Scope,
                    MessageName = trigger.MessageName,
                    MessageTag = trigger.MessageTag,
                    CreationTime = timestamp
                });
            }

            return behaviourTriggerRepository.CreateBulkAsync(entites.ToArray());
        }

        public async Task DeleteStaticAsync(int definitionVersionId)
        {
            unitOfWork.EnsureTransaction();

            // read all triggers
            BehaviourTriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
            await behaviourTriggerRepository.DeleteStaticAsync(definitionVersionId);
            IEfeuTrigger[] triggerInstances = triggers.Select(i => triggerProvider.TryGetTrigger(i.MessageName));
            // get all trigger instances
            // instantiate and call DetatchAsync
        }

        public Task DeleteBulkAsync(Guid[] ids)
        {
            // foreach trigger
            // instantiate and call DetatchAsync
            return behaviourTriggerRepository.DeleteBulkAsync(ids);
        }
    }
}
