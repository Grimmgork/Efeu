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

        public async Task AttachAsync(EfeuTrigger[] triggers, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();

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
                    Matter = trigger.Matter,
                    CreationTime = timestamp
                });
            }

            await behaviourTriggerRepository.CreateBulkAsync(entites.ToArray());

            await unitOfWork.CompleteAsync();
        }

        public async Task DetatchStaticAsync(int definitionVersionId)
        {
            await unitOfWork.BeginAsync();
            // read all triggers
            BehaviourTriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
            await behaviourTriggerRepository.DeleteStaticAsync(definitionVersionId);
            await unitOfWork.CompleteAsync();
        }

        public Task DetatchAsync(Guid[] ids)
        {
            return behaviourTriggerRepository.DeleteBulkAsync(ids);
        }

        public Task ResolveMattersAsync(Guid[] matters)
        {
            return behaviourTriggerRepository.DeleteByMatterBulkAsync(matters);
        }
    }
}
