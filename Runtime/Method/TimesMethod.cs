using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class TimesMethod : WorkflowMethodBase
    {
        public override Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token)
        {
            if (context.IsFirstRun)
            {
                context.Data = 0;
            }

            if (context.Data.ToInt() < context.Input.ToInt())
            {
                context.Data++;
                return Task.FromResult(WorkflowMethodState.Yield);
            }
            else
            {
                return Task.FromResult(WorkflowMethodState.Done);
            }
        }
    }
}
