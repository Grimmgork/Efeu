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
    internal class TriggerCommands : ITriggerCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly ITriggerQueries triggerQueries;
        private readonly IEfeuTriggerProvider triggerProvider;

        public TriggerCommands(IEfeuUnitOfWork unitOfWork, ITriggerQueries triggerQueries, IEfeuTriggerProvider triggerProvider)
        {
            this.unitOfWork = unitOfWork;
            this.triggerQueries = triggerQueries;
            this.triggerProvider = triggerProvider;
        }

        public async Task AttachAsync(EfeuTrigger[] triggers)
        {
            await unitOfWork.BeginAsync();

            List<TriggerEntity> entites = new List<TriggerEntity>();
            foreach (EfeuTrigger trigger in triggers)
            {
                entites.Add(new TriggerEntity()
                {
                    Id = trigger.Id,
                    BehaviourVersionId = trigger.BehaviourId,
                    CorrelationId = trigger.CorrelationId,
                    Position = trigger.Position,
                    Scope = trigger.Scope,
                    Input = trigger.Input,
                    Type = trigger.Type,
                    Tag = trigger.Tag,
                    Matter = trigger.Matter,
                    CreationTime = trigger.CreationTime
                });
            }

            await triggerQueries.CreateBulkAsync(entites.ToArray());

            await unitOfWork.CompleteAsync();
        }

        public async Task DetatchStaticAsync(int definitionVersionId)
        {
            await unitOfWork.BeginAsync();
            // TriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
            await triggerQueries.DeleteStaticAsync(definitionVersionId);
            await unitOfWork.CompleteAsync();
        }

        public Task DetatchAsync(Guid[] ids)
        {
            return triggerQueries.DeleteBulkAsync(ids);
        }

        public Task ResolveMattersAsync(Guid[] matters)
        {
            return triggerQueries.DeleteByMatterBulkAsync(matters);
        }

        public Task CompleteGroupsAsync(Guid[] groups)
        {
            return triggerQueries.DeleteByGroupBulkAsync(groups);
        }
    }
}
