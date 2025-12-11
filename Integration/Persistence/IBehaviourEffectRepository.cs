using Efeu.Integration.Entities;
using System;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourEffectRepository
    {
        public Task<int> CreateAsync(BehaviourEffectEntity entity);

        public Task CreateBulkAsync(BehaviourEffectEntity[] entities);

        public Task DeleteByCorellationAsync(Guid correlationId);

        public Task DeleteAsync(int id);

        public Task<BehaviourEffectEntity?> GetByIdAsync(int id);

        public Task<BehaviourEffectEntity[]> GetByCorellationAsync(Guid correlationId);

        public Task<BehaviourEffectEntity[]> GetAll();

        public Task MarkErrorAsync(int id, uint times);

        public Task<int> NudgeAsync(int id);

        public Task<bool> TryLockAsync(int id, Guid lockId, DateTimeOffset timestamp, TimeSpan lease);

        public Task<BehaviourEffectEntity?> GetRunningSignalAsync();

        public Task<int[]> GetRunningEffectNotLockedAsync(DateTimeOffset time);

        public Task UnlockAsync(Guid lockId);
    }
}
