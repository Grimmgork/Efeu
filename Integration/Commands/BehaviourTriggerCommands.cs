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
        private readonly IUnitOfWork unitOfWork;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;

        public BehaviourTriggerCommands(IUnitOfWork unitOfWork, IBehaviourTriggerRepository behaviourTriggerRepository)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
        }

        public Task CreateAsync(BehaviourTrigger trigger)
        {
            return behaviourTriggerRepository.Add(new BehaviourTriggerEntity()
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

        public Task DeleteStaticAsync(int definitionVersionId)
        {
            return behaviourTriggerRepository.DeleteStaticAsync(definitionVersionId);
        }

        public Task DeleteAsync(Guid id)
        {
            return behaviourTriggerRepository.DeleteAsync(id);
        }

        public Task DeleteByCorrelation(Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
