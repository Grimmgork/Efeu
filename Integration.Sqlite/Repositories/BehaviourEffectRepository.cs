using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Efeu.Integration.Sqlite.Repositories
{
    internal class BehaviourEffectRepository : IBehaviourEffectRepository
    {
        private readonly DataConnection connection;

        public BehaviourEffectRepository(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task CreateAsync(EfeuEffectEntity entity)
        {
            return connection.InsertWithInt32IdentityAsync(entity);
        }

        public Task CreateBulkAsync(EfeuEffectEntity[] entities)
        {
            return connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, entities);
        }

        public Task DeleteEffectAsync(Guid id)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .DeleteAsync(i => i.Id == id
                            && (i.State == BehaviourEffectState.Suspended || i.State == BehaviourEffectState.Faulted));
        }

        public Task<EfeuEffectEntity[]> GetAllAsync()
        {
            return connection.GetTable<EfeuEffectEntity>()
                .ToArrayAsync();
        }

        public Task<EfeuEffectEntity[]> GetByCorellationAsync(Guid correlationId)
        {
            return connection.GetTable<EfeuEffectEntity>()
                 .Where(i => i.CorrelationId == correlationId)
                 .ToArrayAsync();
        }

        public Task<EfeuEffectEntity?> GetByIdAsync(Guid id)
        {
            return connection.GetTable<EfeuEffectEntity>()
                 .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<int> NudgeEffectAsync(Guid id)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.Id == id
                    && (u.State == BehaviourEffectState.Suspended || u.State == BehaviourEffectState.Faulted))
                .Set(u => u.State, BehaviourEffectState.Running)
                .UpdateAsync();
        }

        public Task<int> SuspendEffectAsync(Guid id, DateTimeOffset timestamp)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.Id == id
                    && u.State == BehaviourEffectState.Running
                    && (u.LockedUntil < timestamp))
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .Set(u => u.State, BehaviourEffectState.Suspended)
                .UpdateAsync();
        }

        public async Task<bool> TryLockEffectAsync(Guid id, Guid lockId, DateTimeOffset timestamp, TimeSpan lease)
        {
            int result = await connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.Id == id
                    && u.State == BehaviourEffectState.Running
                    && (u.LockedUntil < timestamp || u.LockId == lockId))
                .Set(u => u.LockId, lockId)
                .Set(u => u.LockedUntil, timestamp + lease)
                .UpdateAsync();
            return result > 0;
        }

        public Task UnlockEffectAsync(Guid lockId)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.LockId == lockId)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task<Guid[]> GetRunningEffectsNotLockedAsync(DateTimeOffset time)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.State == BehaviourEffectState.Running
                    && u.State == BehaviourEffectState.Running
                    && u.LockedUntil < time)
                .OrderBy(u => u.CreationTime)
                .Select(p => p.Id)
                .ToArrayAsync();
        }

        public Task FaultEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, string fault)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.Id == id 
                    && u.LockId == lockId
                    && u.State == BehaviourEffectState.Running)
                .Set(u => u.State, BehaviourEffectState.Faulted)
                .Set(u => u.ExecutionTime, timestamp)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.Matter, id)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .Set(u => u.Fault, fault)
                .UpdateAsync();
        }

        public Task CompleteEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, EfeuValue output)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.Id == id
                    && u.LockId == lockId
                    && u.State == BehaviourEffectState.Running)
                .Set(u => u.Tag, EfeuMessageTag.Result)
                .Set(u => u.Input, output)
                .Set(u => u.CreationTime, timestamp)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.Matter, id)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task CompleteSuspendedEffectAsync(Guid id, DateTimeOffset timestamp, EfeuValue output)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .Where(u => u.Id == id
                    && u.State == BehaviourEffectState.Suspended)
                .Set(u => u.State, BehaviourEffectState.Running)
                .Set(u => u.Input, output)
                .Set(u => u.CreationTime, timestamp)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task DeleteCompletedSignalAsync(Guid id)
        {
            return connection.GetTable<EfeuEffectEntity>()
                .DeleteAsync(u => u.Id == id);
        }
    }
}
