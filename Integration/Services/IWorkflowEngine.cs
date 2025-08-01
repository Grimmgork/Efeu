﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Signal;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System.Threading;

namespace Efeu.Integration.Services
{
    public interface IWorkflowEngine
    {
        public Task<SomeData> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeData input, CancellationToken token = default);

        public Task SendSignal(WorkflowSignal signal);
    }
}
