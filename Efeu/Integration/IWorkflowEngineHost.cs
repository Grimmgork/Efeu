using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public interface IWorkflowEngineHost
    {
        public Task PublishWorkflowDefinitionVersion(WorkflowDefinition definition);
        public Task<WorkflowInstance> StartWorkflowAsync(int id);
        public Task<SomeDataStruct> ExecuteWorkflowAsync(int id, SomeDataStruct input, CancellationToken token = default);
    }
}
