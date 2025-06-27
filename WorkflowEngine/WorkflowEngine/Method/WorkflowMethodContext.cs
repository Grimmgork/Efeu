using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;
using Workflows.Function;
using Workflows.Message;

namespace Workflows.Method
{
    public class WorkflowMethodContext
    {
        public readonly SomeDataStruct Input;

        public SomeDataStruct Output;

        public SomeData Data;

        public string? Route;

        public SomeDataStruct WorkflowOutput;

        public SomeDataStruct Variables;

        private Func<WorkflowSignal, Task> sendSignal;

        public Task RaiseSignal(WorkflowSignal message)
        {
            return sendSignal(message);
        }

        public WorkflowMethodContext(SomeDataStruct variables, SomeDataStruct workflowOutput, SomeDataStruct input, Func<WorkflowSignal, Task> sendSignal)
        {
            Variables = variables;
            Input = input;
            this.sendSignal = sendSignal;
            Output = new SomeDataStruct();
            WorkflowOutput = workflowOutput;
        }

        public WorkflowMethodContext(SomeDataStruct variables, SomeDataStruct workflowOutput, SomeDataStruct input, Func<WorkflowSignal, Task> sendSignal, SomeData data)
        {
            Variables = variables;
            Input = input;
            Data = data;
            Output = new SomeDataStruct();
            this.sendSignal = sendSignal;
            WorkflowOutput = workflowOutput;
        }
    }
}
