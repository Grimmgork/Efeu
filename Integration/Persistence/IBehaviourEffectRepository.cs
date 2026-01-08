using Efeu.Integration.Entities;
using Efeu.Router.Value;
using System;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourEffectRepository
    {
        public Task<int> CreateAsync(BehaviourEffectEntity entity);

        public Task CreateBulkAsync(BehaviourEffectEntity[] entities);

        public Task DeleteEffectAsync(int id);

        public Task DeleteCompletedSignalAsync(int id);

        public Task<BehaviourEffectEntity?> GetByIdAsync(int id);

        public Task<BehaviourEffectEntity[]> GetByCorellationAsync(Guid correlationId);

        public Task<BehaviourEffectEntity[]> GetAllAsync();

        public Task FaultEffectAndUnlockAsync(Guid lockId, int id, DateTimeOffset timestamp, string fault);

        public Task CompleteEffectAndUnlockAsync(Guid lockId, int id, DateTimeOffset timestamp, EfeuValue output);

        public Task<int> SuspendEffectAsync(int id, DateTimeOffset timestamp);

        public Task CompleteSuspendedEffectAsync(int id, DateTimeOffset timestamp, EfeuValue output);

        public Task<int> NudgeEffectAsync(int id);

        public Task<bool> TryLockEffectAsync(int id, Guid lockId, DateTimeOffset timestamp, TimeSpan lease);

        public Task<int[]> GetRunningEffectsNotLockedAsync(DateTimeOffset time);

        public Task UnlockEffectAsync(Guid lockId);
    }
}
