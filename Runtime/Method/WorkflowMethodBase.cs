using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Integration.Logic;
using Efeu.Runtime.Function;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime.Method
{
    public abstract class WorkflowMethodBase : IWorkflowMethod
    {


        public virtual WorkflowMethodState Run(WorkflowMethodContext context)
        {
            return WorkflowMethodState.Done;
        }

        public virtual Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token)
        {
            return Task.FromResult(Run(context));
        }

        public virtual void Attach(WorkflowMethodContext context)
        {

        }

        public virtual Task AttachAsync(WorkflowMethodContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public virtual WorkflowMethodState OnTrigger(WorkflowMethodContext context, object signal)
        {
            return WorkflowMethodState.Running;
        }

        public virtual void Dispose()
        {

        }

        public Task UnattachAsync(WorkflowMethodContext context, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
