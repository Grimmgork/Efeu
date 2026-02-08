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
    internal class BehaviourQueries : IBehaviourQueries
    {
        private readonly DataConnection connection;

        public BehaviourQueries(DataConnection connection) 
        { 
            this.connection = connection;
        }

        public Task<int> CreateVersionAsync(BehaviourVersionEntity version)
        {
            return connection.InsertWithInt32IdentityAsync(version);
        }

        public Task<int> CreateAsync(BehaviourEntity definition)
        {
            return connection.InsertWithInt32IdentityAsync(definition);
        }

        public Task DeleteAsync(int id)
        {
            return connection.GetTable<BehaviourEntity>()
                .DeleteAsync(i => i.Id == id);
        }

        public Task<BehaviourEntity[]> GetAllAsync()
        {
            return connection.GetTable<BehaviourEntity>().ToArrayAsync();
        }

        public Task<BehaviourVersionEntity?> GetVersionByIdAsync(int id)
        {
            return connection.GetTable<BehaviourVersionEntity>()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<BehaviourVersionEntity[]> GetVersionsByIdsAsync(int[] ids)
        {
            if (ids.Length == 0)
                return Task.FromResult<BehaviourVersionEntity[]>([]);

            return connection.GetTable<BehaviourVersionEntity>()
                .Where(i => ids.Contains(i.Id))
                .ToArrayAsync();
        }

        public Task<BehaviourVersionEntity?> GetLatestVersionAsync(int definitionId)
        {
            return connection.GetTable<BehaviourVersionEntity>()
                .OrderByDescending(i => i.Version)
                .FirstOrDefaultAsync(i => i.BehaviourId == definitionId);
        }

        public Task<BehaviourEntity?> GetByIdAsync(int id)
        {
            return connection.GetTable<BehaviourEntity>()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task UpdateAsync(BehaviourEntity entity)
        {
            return connection.UpdateAsync(entity);
        }
    }
}
