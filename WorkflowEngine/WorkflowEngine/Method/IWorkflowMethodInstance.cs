using Workflows.Data;
using Workflows.Message;

namespace Workflows.Method
{
    public interface IWorkflowMethodInstance : IDisposable
    {
        public Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token);

        public WorkflowMethodState OnSignal(WorkflowMethodContext context, WorkflowSignal signal);
    }
}
