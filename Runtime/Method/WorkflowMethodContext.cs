using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Trigger;

namespace Efeu.Runtime.Method
{
    public class WorkflowMethodContext
    {
        public readonly bool IsFirstRun;

        public readonly SomeData Input;

        public readonly SomeData InitialInput;

        public readonly SomeData Result;

        public WorkflowTriggerHash Trigger;

        public SomeData Output;

        public SomeData Data;

        public string? Route;

        public WorkflowMethodContext(SomeData input)
        {
            Input = input;
            Output = new SomeData();
            IsFirstRun = true;
        }

        public WorkflowMethodContext(SomeData input, SomeData data, SomeData dispatchResult)
        {
            Input = input;
            Data = data;
            Output = new SomeData();
            IsFirstRun = false;
            Result = dispatchResult;
        }
    }
}
