using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Efeu.Router;

namespace Efeu.Integration.Sqlite.Repositories
{
    internal class BehaviourTriggerRepository : IBehaviourTriggerRepository
    {
        private readonly SqliteDataConnection connection;

        public BehaviourTriggerRepository(SqliteDataConnection connection)
        {
            this.connection = connection;
        }

        public Task<int> Add(BehaviourTriggerEntity trigger)
        {
            return connection.InsertWithInt32IdentityAsync(trigger);
        }

        public Task DeleteAsync(Guid id)
        {
            return connection.GetTable<BehaviourTriggerEntity>()
                .DeleteAsync(i => i.Id == id);
        }

        public Task DeleteStaticAsync(int definitionVersionId)
        {
            return connection.GetTable<BehaviourTriggerEntity>()
                .DeleteAsync(i => i.DefinitionVersionId == definitionVersionId && i.CorrelationId == Guid.Empty);
        }

        public async Task<IEnumerable<BehaviourTriggerEntity>> GetAllActiveAsync()
        {
             return await connection.GetTable<BehaviourTriggerEntity>()
                .ToArrayAsync();
        }

        public Task<BehaviourTriggerEntity> GetByIdAsync(Guid id)
        {
            return connection.GetTable<BehaviourTriggerEntity>()
                .FirstAsync(i => i.Id == id);
        }

        public Task<BehaviourTriggerEntity[]> GetMatchingAsync(string name, EfeuMessageTag tag, Guid triggerId)
        {
            Console.WriteLine("query");
            return connection.GetTable<BehaviourTriggerEntity>()
                .Where(i => i.MessageName == name && i.MessageTag == tag && ( triggerId == Guid.Empty ? true : i.Id == triggerId) )
                .ToArrayAsync();
        }
    }
}
