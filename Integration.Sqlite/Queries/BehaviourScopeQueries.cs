using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite.Queries
{
    internal class BehaviourScopeQueries : IBehaviourScopeQueries
    {
        private readonly DataConnection connection;

        public BehaviourScopeQueries(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task<BehaviourScopeEntity?> GetByIdAsync(Guid id)
        {
            return connection.GetTable<BehaviourScopeEntity>()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<BehaviourScopeEntity[]> GetByIdsAsync(Guid[] ids)
        {
            if (ids.Length == 0)
            {
                return [];
            }
            else if (ids.Length == 1)
            {
                BehaviourScopeEntity? entity = await GetByIdAsync(ids.First());
                if (entity == null)
                {
                    return [];
                }
                else
                {
                    return [entity];
                }
            }
            else
            {
                return await connection.GetTable<BehaviourScopeEntity>()
                    .Where(u => ids.Contains(u.Id))
                    .ToArrayAsync();
            }
        }

        public Task CreateBulkAsync(BehaviourScopeEntity[] entities)
        {
            return connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, entities);
        }

        public Task DecrementReferenceCountAsync(Guid id)
        {
            return connection.GetTable<BehaviourScopeEntity>()
                .Where(u => u.Id == id
                    && u.ReferenceCount > 0)
                .Set(u => u.ReferenceCount, (c) => c.ReferenceCount - 1)
                .UpdateAsync();
        }

        public Task DeleteUnreferencedAsync()
        {
            return connection.GetTable<BehaviourScopeEntity>()
                .Where(u => u.ReferenceCount == 0)
                .DeleteAsync();
        }
    }
}
