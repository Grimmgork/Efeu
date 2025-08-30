using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

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

        public static Task ContinueAsync(this WorkflowRuntime instance, object signal, CancellationToken token = default)
        {
            instance.Signal(signal);
            return instance.RunAsync(token);
        }

        public static Task TriggerAsync(this WorkflowRuntime instance, int id, object signal, CancellationToken token = default)
        {
            instance.Trigger(id, signal);
            return instance.RunAsync(token);
        }
    }
}
