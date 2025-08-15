using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Signal;
using Efeu.Runtime.Trigger;

namespace Efeu.Runtime.Method
{
    public class WaitForInputMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            Console.WriteLine("Pls enter a value");
            context.Trigger = new ConsoleInputTrigger()
            {
                CallbackId = 1
            };
            return WorkflowMethodState.Suspended;
        }

        public override WorkflowMethodState OnTrigger(WorkflowMethodContext context, object signal)
        {
            if (signal is ConsoleInputSignal consoleSignal)
            {
                context.Output = SomeData.Reference(consoleSignal);
                return WorkflowMethodState.Done;
            }

            return WorkflowMethodState.Suspended;
        }
    }
}
