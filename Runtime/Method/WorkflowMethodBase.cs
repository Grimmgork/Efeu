using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Function;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime.Method
{
    public abstract class WorkflowMethodBase : IWorkflowMethod
    {
        public virtual void Dispose()
        {

        }

        public virtual WorkflowTrigger ConfigureTrigger(WorkflowInitialContext context)
        {
            return WorkflowTrigger.Start();
        }

        public virtual WorkflowMethodState OnTrigger(WorkflowMethodContext context, object signal)
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
