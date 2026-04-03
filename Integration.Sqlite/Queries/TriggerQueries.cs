using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite.Queries
{
    internal class TriggerQueries : ITriggerQueries
    {
        private readonly DataConnection connection;

        public TriggerQueries(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task<int> CreateAsync(TriggerEntity trigger)
        {
            return connection.InsertWithInt32IdentityAsync(trigger);
        }

        public Task CreateBulkAsync(TriggerEntity[] triggers)
        {
            return connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, triggers);
        }

        public Task DetatchAsync(Guid[] ids)
        {
            if (ids.Length == 0)
                return Task.CompletedTask;

            return connection.GetTable<TriggerEntity>()
                .Where(i => ids.Contains(i.Id) && !i.IsDetatched)
                .Set(u => u.IsDetatched, true)
                .UpdateAsync();
        }

        public Task DetatchStaticAsync(int definitionVersionId)
        {
            return connection.GetTable<TriggerEntity>()
                .Where(i => i.BehaviourVersionId == definitionVersionId
                               && i.CorrelationId == Guid.Empty && !i.IsDetatched)
                .Set(u => u.IsDetatched, true)
                .UpdateAsync();
        }

        public Task DetatchByMatterBulkAsync(Guid[] matters)
        {
            if (matters.Length == 0)
                return Task.CompletedTask;

            return connection.GetTable<TriggerEntity>()
                .Where(i => matters.Contains(i.Matter) && !i.IsDetatched)
                .Set(u => u.IsDetatched, true)
                .UpdateAsync();
        }

        public Task DetatchByGroupBulkAsync(Guid[] groups)
        {
            if (groups.Length == 0)
                return Task.CompletedTask;

            return connection.GetTable<TriggerEntity>()
                .Where(i => groups.Contains(i.Group) && !i.IsDetatched)
                .Set(u => u.IsDetatched, true)
                .UpdateAsync();
        }

        public Task<TriggerEntity[]> GetAllAsync()
        {
             return connection.GetTable<TriggerEntity>()
                .Where(i => !i.IsDetatched)
                .ToArrayAsync();
        }

        public Task<TriggerEntity[]> GetStaticAsync(int definitionVersionId)
        {
            return connection.GetTable<TriggerEntity>()
                .Where(i => i.BehaviourVersionId == definitionVersionId
                         && i.CorrelationId == Guid.Empty
                         && !i.IsDetatched)
                .ToArrayAsync();
        }

        public Task<TriggerEntity?> GetByIdAsync(Guid id)
        {
            return connection.GetTable<TriggerEntity>()
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDetatched);
        }

        public async Task<TriggerEntity[]> GetByIdsAsync(params Guid[] ids)
        {
            if (ids.Length == 0)
            {
                return [];
            }
            else if (ids.Length == 1)
            {
                TriggerEntity? entity = await GetByIdAsync(ids.First());
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
                return await connection.GetTable<TriggerEntity>()
                    .Where(i => ids.Contains(i.Id) && !i.IsDetatched)
                    .ToArrayAsync();
            }
        }

        public Task<TriggerEntity[]> GetDetatchedAsync(int limit)
        {
            return connection.GetTable<TriggerEntity>()
                    .Where(i => i.IsDetatched)
                    .Take(limit)
                    .ToArrayAsync();
        }

        public Task DeleteAsync(Guid[] ids)
        {
            return connection.GetTable<TriggerEntity>()
                    .DeleteAsync(i => ids.Contains(i.Id) && i.IsDetatched);
        }
    }
}
