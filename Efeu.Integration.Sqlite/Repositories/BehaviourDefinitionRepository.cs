using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Runtime.Model;
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
        private readonly SqliteDataConnection connection;

        public BehaviourDefinitionRepository(UnitOfWork unitOfWork, SqliteDataConnection connection) 
        { 
            this.connection = connection;
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

        public Task<BehaviourDefinitionEntity> GetByIdAsync(int id)
        {
            return connection.GetTable<BehaviourDefinitionEntity>()
                .FirstAsync(i => i.Id == id);
        }

        public Task<BehaviourDefinitionEntity[]> GetByIdsAsync(int[] ids)
        {
            return connection.GetTable<BehaviourDefinitionEntity>()
                .Where(i => ids.Contains(i.Id))
                .ToArrayAsync();
        }

        public Task<BehaviourDefinitionEntity> GetNewestByNameAsync(string name)
        {
            return connection.GetTable<BehaviourDefinitionEntity>()
                .Where(i => i.Name == name)
                .OrderByDescending(i => i.Version)
                .FirstAsync();
        }
    }
}
