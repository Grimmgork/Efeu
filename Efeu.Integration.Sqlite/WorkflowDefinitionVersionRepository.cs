using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class WorkflowDefinitionVersionRepository : IWorkflowDefinitionVersionRepository
    {
        private readonly SqliteDataConnection connection;

        public WorkflowDefinitionVersionRepository(SqliteDataConnection connection) 
        { 
            this.connection = connection;
        }

        public Task Create(WorkflowDefinitionVersionEntity version)
        {
            return connection.InsertAsync(version);
        }

        public Task DeleteAsync(int id)
        {
            return connection.GetTable<WorkflowDefinitionVersionEntity>()
                    .DeleteAsync(i => i.Id == id);
        }

        public Task<WorkflowDefinitionVersionEntity> GetByIdAsync(int id)
        {
            return connection.GetTable<WorkflowDefinitionVersionEntity>()
                .FirstAsync(i => i.Id == id);
        }

        public Task<WorkflowDefinitionVersionEntity> GetByVersionAsync(int definitionId, int version)
        {
            return connection.GetTable<WorkflowDefinitionVersionEntity>()
                .FirstAsync(i => i.WorkflowDefinitionId == definitionId && i.Version == version);
        }

        public Task<WorkflowDefinitionVersionEntity[]> GetAllVersionsAsync(int definitionId)
        {
            return connection.GetTable<WorkflowDefinitionVersionEntity>()
                .Where(i => i.WorkflowDefinitionId == definitionId).ToArrayAsync();
        }

        public Task<WorkflowDefinitionVersionEntity> GetLatestVersion(int definitionId)
        {
            return connection.GetTable<WorkflowDefinitionVersionEntity>()
                .OrderByDescending(i => i.Version)
                .FirstAsync(i => i.WorkflowDefinitionId == definitionId);
        }
    }
}
