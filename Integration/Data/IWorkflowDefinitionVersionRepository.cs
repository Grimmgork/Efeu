using Efeu.Integration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public interface IWorkflowDefinitionVersionRepository
    {
        public Task<WorkflowDefinitionVersionEntity> GetByIdAsync(int id);

        public Task<WorkflowDefinitionVersionEntity> GetByVersionAsync(int definitionId, int version);

        public Task<WorkflowDefinitionVersionEntity> GetLatestVersion(int definitionId);

        public Task<WorkflowDefinitionVersionEntity[]> GetAllVersionsAsync(int definitionId);

        public Task DeleteAsync(int id);

        public Task Create(WorkflowDefinitionVersionEntity version);
    }
}
