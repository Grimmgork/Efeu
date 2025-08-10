using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime.Method
{
    public class WaitForInputMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            Console.WriteLine("Pls enter a value");
            context.SignalFilter = new WorkflowTrigger(nameof(PromptInputTrigger), 10.ToString());
            return WorkflowMethodState.Suspended;
        }

        public override WorkflowMethodState OnSignal(WorkflowMethodContext context, WorkflowSignal signal)
        {
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
