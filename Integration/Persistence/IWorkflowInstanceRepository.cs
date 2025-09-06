using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IWorkflowInstanceRepository
    {
        public Task<WorkflowInstanceEntity> GetByIdAsync(int id);

        public Task<IEnumerable<WorkflowInstanceEntity>> GetAllActiveAsync();

        public Task<WorkflowInstanceEntity?> GetForProcessing(int id, WorkflowExecutionState[] allowedExecutionStates, WorkflowExecutionState toExecutionState);

        public Task<int> Add(WorkflowInstanceEntity instance);

        public Task Update(WorkflowInstanceEntity instance);

        public Task<bool> Delete(int id, WorkflowExecutionState[] allowedExecutionStates);
    }
}
