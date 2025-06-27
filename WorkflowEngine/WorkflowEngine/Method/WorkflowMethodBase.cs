using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Function;
using Workflows.Message;

namespace Workflows.Method
{
    public abstract class WorkflowMethodBase : IWorkflowMethodInstance
    {
        public virtual void Dispose()
        {

        }

        public virtual WorkflowMethodState OnSignal(WorkflowMethodContext context, WorkflowSignal signal)
        {
            return WorkflowMethodState.Running;
        }

        public virtual WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            return WorkflowMethodState.Done;
        }

        public virtual Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token)
        {
            return Task.FromResult(Run(context, token));
        }
    }
}
