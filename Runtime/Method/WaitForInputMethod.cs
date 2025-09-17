using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;

namespace Efeu.Runtime.Method
{
    public class WaitForInputMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            Console.WriteLine("Pls enter a value");
            context.Trigger = new WorkflowTriggerHash(nameof(ConsoleInputSignal));

            return WorkflowMethodState.Suspended;
        }

        public override WorkflowMethodState Signal(WorkflowMethodContext context, object signal)
        {
            if (signal is ConsoleInputSignal consoleSignal)
            {
                context.Output = consoleSignal.Input;
                return WorkflowMethodState.Done;
            }

            return WorkflowMethodState.Suspended;
        }
    }
}
