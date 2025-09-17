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
        public bool IsFirstRun => Times == 0;

        public readonly int Times;

        public readonly EfeuValue Input;

        public readonly EfeuValue Result;

        public WorkflowTriggerHash Trigger;

        public EfeuValue Output;

        public EfeuValue Data;

        public string? Route;

        public WorkflowMethodContext(EfeuValue input)
        {
            Input = input;
            Output = new EfeuValue();
        }

        public WorkflowMethodContext(EfeuValue input, EfeuValue data, EfeuValue result, int times)
        {
            Input = input;
            Data = data;
            Output = new EfeuValue();
            Result = result;
            Times = times;
        }
    }
}
