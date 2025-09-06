using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.Model;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly SqliteDataConnection connection;

        public WorkflowDefinitionRepository(UnitOfWork unitOfWork, SqliteDataConnection connection) 
        { 
            this.connection = connection;
        }

        public Task<int> CreateAsync(WorkflowDefinitionEntity definition)
        {
            return connection.InsertWithInt32IdentityAsync(definition);
        }

        public Task DeleteAsync(int id)
        {
            return connection.GetTable<WorkflowDefinitionEntity>()
                .DeleteAsync(i => i.Id == id);
        }

        public Task<WorkflowDefinitionEntity[]> GetAllAsync()
        {
            return connection.GetTable<WorkflowDefinitionEntity>().ToArrayAsync();
        }

        public Task<WorkflowDefinitionEntity> GetByIdAsync(int id)
        {
            return connection.GetTable<WorkflowDefinitionEntity>()
                .FirstAsync(i => i.Id == id);
        }

        public Task<WorkflowDefinitionEntity> GetByNameAsync(string name)
        {
            return connection.GetTable<WorkflowDefinitionEntity>()
                .FirstAsync(i => i.Name == name);
        }

        public Task UpdateAsync(WorkflowDefinitionEntity definition)
        {
            return connection.UpdateAsync(definition);
        }
    }
}
