using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime
{
    public static class WorkflowRuntimeExtensions
    {
        public static async Task RunAsync(this WorkflowRuntime instance, CancellationToken token = default)
        {
            while (instance.State == WorkflowRuntimeState.Running)
            {
                await instance.StepAsync(token);
            }
        }
    }
}
