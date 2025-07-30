using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Integration.Interfaces;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using Efeu.Runtime.Signal;

namespace Efeu.Integration
{
    public class WorkflowEngine : IWorkflowEngine
    {
        public WorkflowEngine()
        {

        }

        public async Task<SomeStruct> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeStruct input, CancellationToken token)
        {

            throw new Exception();
        }

        public Task<SomeData> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeData input, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
