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
    public class WorkflowEngine : IWorkflowEngine
    {
        private IWorkflowActionInstanceFactory methodInstanceFactory;
        private IWorkflowInstanceSignalHandler signalHandler;

        public WorkflowEngine(IWorkflowActionInstanceFactory instanceFactory, IWorkflowInstanceSignalHandler signalHandler)
        {
            this.methodInstanceFactory = instanceFactory;
            this.signalHandler = signalHandler;
        }

        public async Task<SomeDataStruct> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeDataStruct input, CancellationToken token = default)
        {
            WorkflowInstance instance = new WorkflowInstance(1, definition, methodInstanceFactory, 
                (signal) => signalHandler.SendSignal(signal), input);

            do
            {
                await instance.RunAsync(token);
            }
            while (instance.State == WorkflowInstanceState.Running);

            if (instance.State != WorkflowInstanceState.Done)
            {
                throw new Exception($"Workflow execution failed {instance.State}");
            }

            WorkflowInstanceData data = instance.Export();
            return data.Output;
        }
    }
}
