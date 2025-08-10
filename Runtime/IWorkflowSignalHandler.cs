using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Function;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime
{
    public interface IWorkflowSignalHandler
    {
        public Task RaiseSignal(CustomWorkflowSignal message);

        public Task<CustomWorkflowSignal> WaitForSignal(CancellationToken token);
    }
}
