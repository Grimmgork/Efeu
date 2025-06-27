using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;
using Workflows.Model;

namespace Workflows.Integration
{
    public interface IWorkflowEngineHost
    {
        public Task PublishWorkflowDefinitionVersion(WorkflowDefinition definition);
        public Task<WorkflowInstance> StartWorkflowAsync(int id);
        public Task<SomeDataStruct> ExecuteWorkflowAsync(int id, SomeDataStruct input, CancellationToken token = default);
    }
}
