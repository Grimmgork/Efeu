using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router.Data;
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
        private readonly SqliteDataConnection connection;

        public BehaviourEffectRepository(SqliteDataConnection connection)
        {
            this.connection = connection;
        }

        public Task<int> CreateAsync(BehaviourEffectEntity entity)
        {
            return connection.InsertWithInt32IdentityAsync(entity);
        }

        public Task DeleteAsync(int id)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .DeleteAsync(i => i.Id == id);
        }

        public Task DeleteByCorellationAsync(Guid correlationId)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .DeleteAsync(i => i.CorrelationId == correlationId);
        }

        public Task<BehaviourEffectEntity[]> GetAllAsync()
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .ToArrayAsync();
        }

        public Task<BehaviourEffectEntity[]> GetByCorellationAsync(Guid correlationId)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                 .Where(i => i.CorrelationId == correlationId)
                 .ToArrayAsync();
        }

        public Task<BehaviourEffectEntity?> GetByIdAsync(int id)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                 .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<int> NudgeAsync(int id)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.Id == id
                    && u.State == BehaviourEffectState.Error)
                .Set(u => u.State, BehaviourEffectState.Running)
                .UpdateAsync();
        }

        public async Task<bool> TryLockAsync(int id, Guid lockId, DateTimeOffset timestamp, TimeSpan lease)
        {
            int result = await connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.Id == id
                    && u.Tag == BehaviourEffectTag.Effect
                    && (u.LockedUntil < timestamp || u.LockId == lockId))
                .Set(u => u.LockId, lockId)
                .Set(u => u.LockedUntil, timestamp + lease)
                .UpdateAsync();
            return result > 0;
        }

        public Task UnlockAsync(Guid lockId)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.LockId == lockId)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task<int[]> GetRunningEffectsNotLockedAsync(DateTimeOffset time)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.State == BehaviourEffectState.Running
                    && u.LockedUntil < time
                    && u.Tag == BehaviourEffectTag.Effect)
                .Select(p => p.Id).ToArrayAsync();
        }

        public Task<BehaviourEffectEntity?> GetRunningSignalAsync()
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.State == BehaviourEffectState.Running 
                    && u.Tag == BehaviourEffectTag.Signal)
                .FirstOrDefaultAsync();
        }

        public Task CreateBulkAsync(BehaviourEffectEntity[] entities)
        {
            return connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, entities);
        }

        public Task MarkErrorAndUnlockAsync(Guid lockId, int id, uint times)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.Id == id 
                    && u.LockId == lockId
                    && u.Tag == BehaviourEffectTag.Effect)
                .Set(u => u.Times, times)
                .Set(u => u.State, BehaviourEffectState.Error)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task CompleteAsync(Guid lockId, int id, DateTimeOffset timestamp, EfeuValue output, uint times)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.Id == id
                    && u.LockId == lockId
                    && u.Tag == BehaviourEffectTag.Effect)
                .Set(u => u.State, BehaviourEffectState.Running)
                .Set(u => u.Tag, BehaviourEffectTag.Signal)
                .Set(u => u.Input, output)
                .Set(u => u.CreationTime, timestamp)
                .Set(u => u.Times, times)
                .Set(u => u.LockId, Guid.Empty)
                .Set(u => u.LockedUntil, DateTimeOffset.MinValue)
                .UpdateAsync();
        }

        public Task MarkErrorAsync(int id, uint times)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                .Where(u => u.Id == id
                    && u.Tag == BehaviourEffectTag.Signal)
                .Set(u => u.Times, times)
                .Set(u => u.State, BehaviourEffectState.Error)
                .UpdateAsync();
        }
    }
}
