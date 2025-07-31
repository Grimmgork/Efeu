using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using Efeu.Runtime.Signal;

namespace Efeu.Integration.Services
{
    public class WorkflowEngine : IWorkflowEngine
    {
        public WorkflowEngine()
        {

        }

        public async Task<SomeData> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeData input, CancellationToken token)
        {
            WorkflowInstance instance = new WorkflowInstance(definition, null, input);
            await instance.RunAsync(token);
            WorkflowInstanceData data = instance.Export();

            if (data.State != WorkflowInstanceState.Done)
                throw new Exception("Workflow has terminated without completion!");

            return data.Output;
        }

        public Task SendSignal(WorkflowSignal signal)
        {
            throw new NotImplementedException();
        }
    }
}
