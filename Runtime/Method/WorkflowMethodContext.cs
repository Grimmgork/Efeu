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
        public SomeStruct Input;

        public SomeStruct Output;

        public SomeData Data;

        public string? Route;

        public SomeStruct WorkflowOutput;

        public SomeStruct Variables;

        public readonly bool InitialRun;

        private Action runDo;

        public void Do()
        {
            runDo();
        }

        public WorkflowMethodContext(SomeStruct variables, SomeStruct workflowOutput, SomeStruct input)
        {
            Variables = variables;
            Input = input;
            Output = new SomeStruct();
            WorkflowOutput = workflowOutput;
            InitialRun = true;
        }

        public WorkflowMethodContext(SomeStruct variables, SomeStruct workflowOutput, SomeStruct input, SomeData data)
        {
            Variables = variables;
            Input = input;
            Data = data;
            Output = new SomeStruct();
            WorkflowOutput = workflowOutput;
            InitialRun = false;
        }
    }
}
