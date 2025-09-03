using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;

namespace Efeu.Runtime
{
    public static class WorkflowRuntimeExtensions
    {
        public static async Task RunAsync(this WorkflowRuntime instance, CancellationToken token = default)
        {
            while (instance.State == WorkflowRuntimeState.Running || instance.State == WorkflowRuntimeState.Initial)
            {
                await instance.StepAsync(token);
            }
        }

        public static Task ContinueAsync(this WorkflowRuntime instance, WorkflowTriggerHash hash, object signal, CancellationToken token = default)
        {
            instance.Signal(hash, signal);
            return instance.RunAsync(token);
        }

        public static Task TriggerAsync(this WorkflowRuntime instance, int id, object signal, CancellationToken token = default)
        {
            instance.Trigger(id, signal);
            return instance.RunAsync(token);
        }
    }
}
