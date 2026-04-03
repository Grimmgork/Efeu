using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourScopeQueries
    {
        public Task<BehaviourScopeEntity?> GetByIdAsync(Guid id);

        public Task<BehaviourScopeEntity[]> GetByIdsAsync(Guid[] id);

        public Task CreateBulkAsync(BehaviourScopeEntity[] entities);

        public Task DecrementReferenceCountAsync(Dictionary<Guid, uint> decrements);

        public Task CleanupAsync();
    }
}
