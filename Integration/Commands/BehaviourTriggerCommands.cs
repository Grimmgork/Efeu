using Efeu.Integration.Entities;
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

        public BehaviourTriggerCommands(IEfeuUnitOfWork unitOfWork, IBehaviourTriggerRepository behaviourTriggerRepository)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
        }

        public Task CreateAsync(BehaviourTrigger trigger)
        {
            return CreateBulkAsync([trigger]);
        }

        public Task CreateBulkAsync(BehaviourTrigger[] triggers)
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
                });
            }

            return behaviourTriggerRepository.CreateBulkAsync(entites.ToArray());
        }

        public Task DeleteStaticAsync(int definitionVersionId)
        {
            return behaviourTriggerRepository.DeleteStaticAsync(definitionVersionId);
        }

        public Task DeleteBulkAsync(Guid[] ids)
        {
            return behaviourTriggerRepository.DeleteBulkAsync(ids);
        }
    }
}
