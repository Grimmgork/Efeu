using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;
using Workflows.Message;
using Workflows.Signal;

namespace Workflows.Method
{
    public class WaitForInputMethod : WorkflowMethodBase
    {
        public override async Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token)
        {
            await context.RaiseSignal(new RequestInputSignal(DateTime.Now));
            return WorkflowMethodState.Suspended;
        }

        public override WorkflowMethodState OnSignal(WorkflowMethodContext context, WorkflowSignal signal)
        {
            if (signal is PromptInputSignal inputSignal)
            {
                context.Output["Result"] = SomeData.String(inputSignal.Input);
                return WorkflowMethodState.Done;
            }
            else
            {
                return WorkflowMethodState.Suspended;
            }
        }
    }
}
