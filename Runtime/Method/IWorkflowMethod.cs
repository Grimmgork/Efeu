using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public interface IWorkflowMethod : IDisposable
    {
        public Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token);

        public WorkflowMethodState Signal(WorkflowMethodContext context, object signal);
    }
}
