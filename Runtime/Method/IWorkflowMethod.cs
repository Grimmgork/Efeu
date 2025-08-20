using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Signal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public interface IWorkflowMethod : IDisposable
    {
        public Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token);

        public WorkflowMethodState OnTrigger(WorkflowMethodContext context, object signal);
    }
}
