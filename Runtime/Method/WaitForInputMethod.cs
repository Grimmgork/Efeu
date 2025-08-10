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

        public override WorkflowMethodState OnSignal(WorkflowMethodContext context, object signal)
        {
            if (signal is ConsoleInputSignal)
            {

            }

            context.Output = signal.Payload;
            return WorkflowMethodState.Done;

            //if (signal is PromptInputTrigger inputSignal)
            //{
            //    context.Output = SomeData.String(inputSignal.Input);
            //    return WorkflowMethodState.Done;
            //}
            //else
            //{
            //    return WorkflowMethodState.Suspended;
            //}
        }
    }
}
