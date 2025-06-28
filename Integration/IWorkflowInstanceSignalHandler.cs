using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Message;

namespace Efeu.Integration
{
    public interface IWorkflowInstanceSignalHandler
    {
        public Task<WorkflowSignal> WaitForIncomingSignal(CancellationToken token);

        public Task SendSignal(WorkflowSignal signal);
    }
}
