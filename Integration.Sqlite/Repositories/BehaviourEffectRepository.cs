using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public Task<BehaviourEffectEntity[]> GetAll()
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

        public Task<BehaviourEffectEntity[]> GetRunningAsync(int limit)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                    .Take(limit)
                    .Where(i => i.State == BehaviourEffectState.Running)
                    .OrderBy(i => i.CreationTime)
                    .ToArrayAsync();
        }

        public Task<BehaviourEffectEntity?> GetByIdAsync(int id)
        {
            return connection.GetTable<BehaviourEffectEntity>()
                 .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task UpdateAsync(BehaviourEffectEntity effect)
        {
            return connection.UpdateAsync(effect);
        }

        public Task CreateBulkAsync(BehaviourEffectEntity[] entities)
        {
            return connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, entities);
        }
    }
}
