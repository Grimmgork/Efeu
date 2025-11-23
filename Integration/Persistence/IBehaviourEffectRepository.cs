using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourEffectRepository
    {
        public Task<int> CreateAsync(BehaviourEffectEntity entity);

        public Task CreateBulkAsync(BehaviourEffectEntity[] entities);

        public Task DeleteByCorellationAsync(Guid correlationId);

        public Task DeleteAsync(int id);

        public Task<BehaviourEffectEntity> GetByIdAsync(int id);

        public Task<BehaviourEffectEntity[]> GetByCorellationAsync(Guid correlationId);

        public Task<BehaviourEffectEntity[]> GetAll();

        public Task UpdateAsync(BehaviourEffectEntity effect);

        public Task<BehaviourEffectEntity[]> GetRunningAsync(int limit);
    }
}
