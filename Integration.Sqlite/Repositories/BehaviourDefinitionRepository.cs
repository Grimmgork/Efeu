using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite.Repositories
{
    internal class BehaviourDefinitionRepository : IBehaviourDefinitionRepository
    {
        private readonly DataConnection connection;

        public BehaviourDefinitionRepository(UnitOfWork unitOfWork, DataConnection connection) 
        { 
            this.connection = connection;
        }

        public Task<int> CreateVersionAsync(BehaviourDefinitionVersionEntity version)
        {
            return connection.InsertWithInt32IdentityAsync(version);
        }

        public Task<int> CreateAsync(BehaviourDefinitionEntity definition)
        {
            return connection.InsertWithInt32IdentityAsync(definition);
        }

        public Task DeleteAsync(int id)
        {
            return connection.GetTable<BehaviourDefinitionEntity>()
                .DeleteAsync(i => i.Id == id);
        }

        public Task<BehaviourDefinitionEntity[]> GetAllAsync()
        {
            return connection.GetTable<BehaviourDefinitionEntity>().ToArrayAsync();
        }

        public Task<BehaviourDefinitionVersionEntity?> GetVersionByIdAsync(int id)
        {
            return connection.GetTable<BehaviourDefinitionVersionEntity>()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<BehaviourDefinitionVersionEntity[]> GetVersionsByIdsAsync(int[] ids)
        {
            if (ids.Length == 0)
                return Task.FromResult<BehaviourDefinitionVersionEntity[]>([]);

            return connection.GetTable<BehaviourDefinitionVersionEntity>()
                .Where(i => ids.Contains(i.Id))
                .ToArrayAsync();
        }

        public Task<BehaviourDefinitionVersionEntity?> GetLatestVersionAsync(int definitionId)
        {
            return connection.GetTable<BehaviourDefinitionVersionEntity>()
                .OrderByDescending(i => i.Version)
                .FirstOrDefaultAsync(i => i.DefinitionId == definitionId);
        }

        public Task<BehaviourDefinitionEntity?> GetByIdAsync(int id)
        {
            return connection.GetTable<BehaviourDefinitionEntity>()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task UpdateAsync(BehaviourDefinitionEntity entity)
        {
            return connection.UpdateAsync(entity);
        }
    }
}
