using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;
using Workflows.Model;
using Workflows.Signal;

namespace Workflows.Integration
{
    public interface IWorkflowEngine
    {
        public Task<SomeDataStruct> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeDataStruct input, CancellationToken token = default);
    }
}
