using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Function;
using Workflows.Message;

namespace Workflows
{
    public interface IWorkflowSignalHandler
    {
        public Task RaiseSignal(WorkflowSignal message);

        public Task<WorkflowSignal> WaitForSignal(CancellationToken token);
    }
}
