﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Message;

namespace Efeu.Integration
{
    internal class WorkflowInstanceSignalHandler : IWorkflowInstanceSignalHandler
    {
        public Task SendSignal(WorkflowSignal signal)
        {
            throw new NotImplementedException();
        }

        public Task<WorkflowSignal> WaitForIncomingSignal(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
