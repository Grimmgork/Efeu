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

        public Task DeleteAsync(Guid id)
        {
            return connection.GetTable<TriggerEntity>()
                .DeleteAsync(i => i.Id == id);
        }

        public Task DeleteBulkAsync(Guid[] ids)
        {
            if (ids.Length == 0)
                return Task.CompletedTask;

            return connection.GetTable<TriggerEntity>()
                .DeleteAsync(i => ids.Contains(i.Id));
        }

        public Task DeleteStaticAsync(int definitionVersionId)
        {
            return connection.GetTable<TriggerEntity>()
                .DeleteAsync(i => i.BehaviourVersionId == definitionVersionId 
                               && i.CorrelationId == Guid.Empty);
        }

        public Task DeleteByMatterBulkAsync(Guid[] matters)
        {
            if (matters.Length == 0)
                return Task.CompletedTask;

            return connection.GetTable<TriggerEntity>()
                .DeleteAsync(i => matters.Contains(i.Matter));
        }

        public Task DeleteByGroupBulkAsync(Guid[] groups)
        {
            if (groups.Length == 0)
                return Task.CompletedTask;

            return connection.GetTable<TriggerEntity>()
                .DeleteAsync(i => groups.Contains(i.Group));
        }

        public Task<TriggerEntity[]> GetAllAsync()
        {
             return connection.GetTable<TriggerEntity>()
                .ToArrayAsync();
        }

        public Task<TriggerEntity[]> GetStaticAsync(int definitionVersionId)
        {
            return connection.GetTable<TriggerEntity>()
                .Where(i => i.BehaviourVersionId == definitionVersionId
                         && i.CorrelationId == Guid.Empty)
                .ToArrayAsync();
        }

        public Task<TriggerEntity> GetByIdAsync(Guid id)
        {
            return connection.GetTable<TriggerEntity>()
                .FirstAsync(i => i.Id == id);
        }

        public Task<TriggerEntity[]> GetByIdsAsync(params Guid[] ids)
        {
            if (ids.Length == 0)
                return Task.FromResult<TriggerEntity[]>([]);

            return connection.GetTable<TriggerEntity>()
                .Where(i => ids.Contains(i.Id))
                .ToArrayAsync();
        }

        public Task<PartialTriggerEntity[]> GetPartialTriggersAsync()
        {
            return connection.GetTable<PartialTriggerEntity>()
                .ToArrayAsync();
        }
    }
}
