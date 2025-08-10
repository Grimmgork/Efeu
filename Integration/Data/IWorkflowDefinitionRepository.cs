using Efeu.Integration.Model;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public interface IWorkflowDefinitionRepository
    {
        public Task<WorkflowDefinitionEntity> GetByNameAsync(string name);

        public Task<WorkflowDefinitionEntity> GetByIdAsync(int id);

        public Task UpdateAsync(WorkflowDefinitionEntity definition);

        public Task<WorkflowDefinitionEntity[]> GetAllAsync();

        public Task<int> CreateAsync(WorkflowDefinitionEntity definition);

        public Task DeleteAsync(int id);
    }
}
