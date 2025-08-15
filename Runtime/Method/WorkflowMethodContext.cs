using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime.Method
{
    public class WorkflowMethodContext
    {
        public readonly SomeData Input;

        public readonly SomeData InitialInput;

        public SomeData Output;

        public SomeData Data;

        public string? Route;

        public IWorkflowTrigger Trigger; // TODO

        public readonly SomeData DispatchResult;

        public readonly bool InitialRun;

        public WorkflowMethodContext(SomeData input)
        {
            Input = input;
            Output = new SomeData();
            InitialRun = true;
        }

        public WorkflowMethodContext(SomeData input, SomeData data, SomeData dispatchResult)
        {
            Input = input;
            Data = data;
            Output = new SomeData();
            InitialRun = false;
            DispatchResult = dispatchResult;
        }
    }
}
