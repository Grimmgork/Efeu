using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
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

        public Task AttachAsync(EfeuTrigger[] triggers, DateTimeOffset timestamp)
        {
            List<BehaviourTriggerEntity> entites = new List<BehaviourTriggerEntity>();
            foreach (EfeuTrigger trigger in triggers)
            {
                entites.Add(new BehaviourTriggerEntity()
                {
                    Id = trigger.Id,
                    DefinitionVersionId = trigger.DefinitionId,
                    CorrelationId = trigger.CorrelationId,
                    Position = trigger.Position,
                    Scope = trigger.Scope,
                    Input = trigger.Input,
                    Name = trigger.Name,
                    Tag = trigger.Tag,
                    CreationTime = timestamp
                });
            }

            return behaviourTriggerRepository.CreateBulkAsync(entites.ToArray());
        }

        public async Task DetatchStaticAsync(int definitionVersionId)
        {
            unitOfWork.EnsureTransaction();

            // read all triggers
            BehaviourTriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
            await behaviourTriggerRepository.DeleteStaticAsync(definitionVersionId);
            List<IEfeuTrigger> instances = new List<IEfeuTrigger>();
            foreach (BehaviourTriggerEntity entity in triggers)
            {
                IEfeuTrigger? instance = triggerProvider.TryGetTrigger(entity.Name);
                if (instance == null)
                    throw new Exception("no trigger provider found for ");
                
                // TODO
                instances.Add(instance);
            }

            // get all trigger instances
            // instantiate and call DetatchAsync
        }

        public Task DetatchAsync(Guid[] ids)
        {
            // foreach trigger
            // instantiate and call DetatchAsync
            return behaviourTriggerRepository.DeleteBulkAsync(ids);
        }
    }
}
