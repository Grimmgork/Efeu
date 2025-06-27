using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Message;

namespace Workflows.Integration
{
    public interface IWorkflowInstanceSignalHandler
    {
        public Task<WorkflowSignal> WaitForIncomingSignal(CancellationToken token);

        public Task SendSignal(WorkflowSignal signal);
    }
}
