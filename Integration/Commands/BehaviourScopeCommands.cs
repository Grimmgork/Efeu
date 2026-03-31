using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class BehaviourScopeCommands : IBehaviourScopeCommands
    {
        private readonly IBehaviourScopeQueries behaviourScopeQueries;

        public BehaviourScopeCommands(IBehaviourScopeQueries behaviourScopeQueries)
        {
            this.behaviourScopeQueries = behaviourScopeQueries;
        }

        public Task CreateBulkAsync(BehaviourScopeEntity[] entities)
        {
            return behaviourScopeQueries.CreateBulkAsync(entities);
        }

        public Task DecrementReferenceCountAsync(Guid id)
        {
            return behaviourScopeQueries.DecrementReferenceCountAsync(id);
        }

        public Task DeleteUnreferencedAsync()
        {
            return behaviourScopeQueries.DeleteUnreferencedAsync();
        }
    }
}
