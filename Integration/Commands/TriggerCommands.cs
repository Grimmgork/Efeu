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

        public async Task CreateAsync(EfeuTrigger[] triggers)
        {
            await unitOfWork.BeginAsync();
            TriggerEntity[] entites = triggers.Select(i => i.MapToTriggerEntity()).ToArray();
            await triggerQueries.CreateBulkAsync(entites);
            await unitOfWork.CompleteAsync();
        }

        public async Task DetatchStaticAsync(int definitionVersionId)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync("Trigger");
            // TriggerEntity[] triggers = await behaviourTriggerRepository.GetStaticAsync(definitionVersionId);
            await triggerQueries.DeleteStaticAsync(definitionVersionId);
            await unitOfWork.CompleteAsync();
        }

        public Task DeleteAsync(Guid[] ids)
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
