using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Message;

namespace Efeu.Runtime.Method
{
    public class WorkflowMethodContext
    {
        public readonly SomeStruct Input;

        public SomeStruct Output;

        public SomeData Data;

        public string? Route;

        public SomeStruct WorkflowOutput;

        public SomeStruct Variables;

        private Func<WorkflowSignal, Task> sendSignal;

        public Task RaiseSignal(WorkflowSignal message)
        {
            return sendSignal(message);
        }

        public WorkflowMethodContext(SomeStruct variables, SomeStruct workflowOutput, SomeStruct input, Func<WorkflowSignal, Task> sendSignal)
        {
            Variables = variables;
            Input = input;
            this.sendSignal = sendSignal;
            Output = new SomeStruct();
            WorkflowOutput = workflowOutput;
        }

        public WorkflowMethodContext(SomeStruct variables, SomeStruct workflowOutput, SomeStruct input, Func<WorkflowSignal, Task> sendSignal, SomeData data)
        {
            Variables = variables;
            Input = input;
            Data = data;
            Output = new SomeStruct();
            this.sendSignal = sendSignal;
            WorkflowOutput = workflowOutput;
        }
    }
}
