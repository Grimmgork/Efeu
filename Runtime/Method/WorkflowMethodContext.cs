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
        public readonly SomeData Input;

        public SomeData Output;

        public SomeData Data;

        public string? Route;

        public SomeData WorkflowOutput;

        public SomeData DispatchContext;

        public SomeStruct Variables;

        public readonly bool InitialRun;

        public WorkflowMethodContext(SomeStruct variables, SomeData input, SomeData workflowOutput)
        {
            Variables = variables;
            Input = input;
            Output = new SomeData();
            InitialRun = true;
            WorkflowOutput = workflowOutput;
        }

        public WorkflowMethodContext(SomeStruct variables, SomeData input, SomeData workflowOutput, SomeData data)
        {
            Variables = variables;
            Input = input;
            Data = data;
            Output = new SomeData();
            InitialRun = false;
            WorkflowOutput = workflowOutput;
        }
    }
}
