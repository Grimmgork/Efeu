using Efeu.Runtime.Data;
using Efeu.Runtime.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public interface IWorkflowMethodInstance : IDisposable
    {
        public Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token);

        public WorkflowMethodState OnSignal(WorkflowMethodContext context, WorkflowSignal signal);
    }
}
