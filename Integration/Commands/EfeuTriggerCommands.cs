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
        private readonly IEfeuTriggerQueries behaviourTriggerQueries;
        private readonly IEfeuTriggerProvider triggerProvider;

        public EfeuTriggerCommands(IEfeuUnitOfWork unitOfWork, IEfeuTriggerQueries behaviourTriggerQueries, IEfeuTriggerProvider triggerProvider)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourTriggerQueries = behaviourTriggerQueries;
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
                    Type = trigger.Type,
                    Tag = trigger.Tag,
                    Matter = trigger.Matter,
                    CreationTime = timestamp
                });
            }

            await behaviourTriggerQueries.CreateBulkAsync(entites.ToArray());

            await unitOfWork.CompleteAsync();
        }

        public async Task DetatchStaticAsync(int definitionVersionId)
        {
            await unitOfWork.BeginAsync();
            // TriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
            await behaviourTriggerQueries.DeleteStaticAsync(definitionVersionId);
            await unitOfWork.CompleteAsync();
        }

        public Task DetatchAsync(Guid[] ids)
        {
            return behaviourTriggerQueries.DeleteBulkAsync(ids);
        }

        public Task ResolveMattersAsync(Guid[] matters)
        {
            return behaviourTriggerQueries.DeleteByMatterBulkAsync(matters);
        }
    }
}
