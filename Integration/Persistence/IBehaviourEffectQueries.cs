using Efeu.Integration.Entities;
using Efeu.Runtime.Value;
using System;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourEffectQueries
    {
        public Task CreateAsync(EffectEntity entity);

        public Task CreateBulkAsync(EffectEntity[] entities);

        public Task AbortEffectAsync(Guid id);

        public Task DeleteCompletedEffectAsync(Guid lockId, Guid id);

        public Task<EffectEntity?> GetByIdAsync(Guid id);

        public Task<EffectEntity[]> GetByCorellationAsync(Guid correlationId);

        public Task<EffectEntity[]> GetAllAsync();

        public Task FaultEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, string fault);

        public Task CompleteEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, EfeuValue output);

        public Task<int> SuspendEffectAsync(Guid id, DateTimeOffset timestamp);

        public Task CompleteSuspendedEffectAsync(Guid id, DateTimeOffset timestamp, EfeuValue output);

        public Task<int> NudgeEffectAsync(Guid id);

        public Task<bool> TryLockEffectAsync(Guid id, Guid lockId, TimeSpan lease);

        public Task<Guid[]> GetRunningEffectsNotLockedAsync();

        public Task UnlockEffectAsync(Guid lockId);
    }
}
