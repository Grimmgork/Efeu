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

namespace Efeu.Integration.Sqlite.Queries
{
    internal class EffectQueries : IEffectQueries
    {
        private readonly DataConnection connection;

        public EffectQueries(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task CreateAsync(EffectEntity entity)
        {
            return connection.InsertWithInt32IdentityAsync(entity);
        }

        public Task CreateBulkAsync(EffectEntity[] entities)
        {
            return connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, entities);
        }

        public Task AbortEffectAsync(Guid id)
        {
            return connection.GetTable<EffectEntity>()
                .DeleteAsync(i => i.Id == id
                            && (i.State == EffectState.Suspended || i.State == EffectState.Faulted));
        }

        public Task<EffectEntity[]> GetAllAsync()
        {
            return connection.GetTable<EffectEntity>()
                .ToArrayAsync();
        }

        public Task<EffectEntity[]> GetByCorellationAsync(Guid correlationId)
        {
            return connection.GetTable<EffectEntity>()
                 .Where(i => i.CorrelationId == correlationId)
                 .ToArrayAsync();
        }

        public Task<EffectEntity?> GetByIdAsync(Guid id)
        {
            return connection.GetTable<EffectEntity>()
                 .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<int> NudgeEffectAsync(Guid id)
        {
            return connection.GetTable<EffectEntity>()
                .Where(u => u.Id == id
                    && (u.State == EffectState.Suspended || u.State == EffectState.Faulted))
                .Set(u => u.State, EffectState.Running)
                .UpdateAsync();
        }

        public Task<int> SuspendEffectAsync(Guid id, DateTimeOffset timestamp)
        {
            return connection.GetTable<EffectEntity>()
                .Where(u => u.Id == id
                    && u.State == EffectState.Running
                    && (u.LockedUntil < timestamp))
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .Set(u => u.State, EffectState.Suspended)
                .UpdateAsync();
        }

        public async Task<bool> TryLockEffectAsync(Guid id, Guid lockId, TimeSpan lease)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            int result = await connection.GetTable<EffectEntity>()
                .Where(u => u.Id == id
                    && u.State == EffectState.Running
                    && (u.LockedUntil < time || u.LockId == lockId))
                .Set(u => u.LockId, lockId)
                .Set(u => u.LockedUntil, time + lease)
                .UpdateAsync();
            return result > 0;
        }

        public Task UnlockEffectAsync(Guid lockId)
        {
            return connection.GetTable<EffectEntity>()
                .Where(u => u.LockId == lockId)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task<Guid[]> GetRunningEffectsNotLockedAsync()
        {
            DateTimeOffset time = DateTimeOffset.Now;
            return connection.GetTable<EffectEntity>()
                .Where(u => u.State == EffectState.Running
                    && u.State == EffectState.Running
                    && u.LockedUntil < time)
                .OrderBy(u => u.CreationTime)
                .Select(p => p.Id)
                .ToArrayAsync();
        }

        public Task FaultEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, string fault)
        {
            return connection.GetTable<EffectEntity>()
                .Where(u => u.Id == id 
                    && u.LockId == lockId
                    && u.State == EffectState.Running)
                .Set(u => u.State, EffectState.Faulted)
                .Set(u => u.ExecutionTime, timestamp)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .Set(u => u.Fault, fault)
                .UpdateAsync();
        }

        public Task CompleteEffectAndUnlockAsync(Guid lockId, Guid id, DateTimeOffset timestamp, EfeuValue output)
        {
            return connection.GetTable<EffectEntity>()
                .Where(u => u.Id == id
                    && u.LockId == lockId
                    && u.State == EffectState.Running)
                .Set(u => u.Tag, EfeuMessageTag.Result)
                .Set(u => u.Input, output)
                .Set(u => u.Type, "")
                .Set(u => u.CreationTime, timestamp)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task CompleteSuspendedEffectAsync(Guid id, DateTimeOffset timestamp, EfeuValue output)
        {
            return connection.GetTable<EffectEntity>()
                .Where(u => u.Id == id
                    && u.State == EffectState.Suspended)
                .Set(u => u.State, EffectState.Running)
                .Set(u => u.Tag, EfeuMessageTag.Result)
                .Set(u => u.Input, output)
                .Set(u => u.CreationTime, timestamp)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task DeleteCompletedEffectAsync(Guid lockId, Guid id)
        {
            return connection.GetTable<EffectEntity>()
                .DeleteAsync(u => u.Id == id
                               && u.LockId == lockId);
        }
    }
}
