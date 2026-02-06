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
    internal class EfeuTriggerCommands : IEfeuTriggerCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IEfeuTriggerRepository behaviourTriggerRepository;
        private readonly IEfeuTriggerProvider triggerProvider;

        public EfeuTriggerCommands(IEfeuUnitOfWork unitOfWork, IEfeuTriggerRepository behaviourTriggerRepository, IEfeuTriggerProvider triggerProvider)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.triggerProvider = triggerProvider;
        }

        public async Task AttachAsync(EfeuTrigger[] triggers, DateTimeOffset timestamp)
        {
            await unitOfWork.BeginAsync();

            List<TriggerEntity> entites = new List<TriggerEntity>();
            foreach (EfeuTrigger trigger in triggers)
            {
                entites.Add(new TriggerEntity()
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
            TriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
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
