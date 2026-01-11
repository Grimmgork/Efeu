using Efeu.Integration.Entities;
using Efeu.Router.Value;
using System;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IBehaviourEffectRepository
    {
        public Task CreateAsync(EfeuEffectEntity entity);

        public Task CreateBulkAsync(EfeuEffectEntity[] entities);

        public Task DeleteEffectAsync(Guid id);

        public Task DeleteCompletedSignalAsync(Guid id);

        public Task<EfeuEffectEntity?> GetByIdAsync(Guid id);

        public Task<EfeuEffectEntity[]> GetByCorellationAsync(Guid correlationId);

        public Task<EfeuEffectEntity[]> GetAllAsync();

        public Task FaultEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, string fault);

        public Task CompleteEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, EfeuValue output);

        public Task<int> SuspendEffectAsync(Guid id, DateTimeOffset timestamp);

        public Task CompleteSuspendedEffectAsync(Guid id, DateTimeOffset timestamp, EfeuValue output);

        public Task<int> NudgeEffectAsync(Guid id);

        public Task<bool> TryLockEffectAsync(Guid id, Guid lockId, DateTimeOffset timestamp, TimeSpan lease);

        public Task<Guid[]> GetRunningEffectsNotLockedAsync(DateTimeOffset time);

        public Task UnlockEffectAsync(Guid lockId);
    }
}
