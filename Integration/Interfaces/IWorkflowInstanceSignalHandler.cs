using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Signal;

namespace Efeu.Integration.Interfaces
{
    public interface IWorkflowInstanceSignalHandler
    {
        public Task<WorkflowSignal> WaitForIncomingSignal(CancellationToken token);

        public Task SendSignal(WorkflowSignal signal);
    }
}
