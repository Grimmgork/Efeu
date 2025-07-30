using Efeu.Integration.Interfaces;
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
    public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly DataConnection connection;

        public WorkflowDefinitionRepository(SqliteUnitOfWork unitOfWork, DataConnection connection) 
        { 
            this.connection = connection;
        }

        public Task GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowDefinition> GetById(int id)
        {
            return connection.GetTable<WorkflowDefinition>()
                .FirstAsync(definition => definition.Id == id);
        }

        public Task GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task Update()
        {
            throw new NotImplementedException();
        }
    }
}
