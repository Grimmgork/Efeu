using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;
using Workflows.Signal;

namespace Workflows
{
    public static class WorkflowInstanceExtensions
    {
        public static async Task RunAsync(this WorkflowInstance instance, CancellationToken token = default)
        {
            while (instance.State == WorkflowInstanceState.Running || instance.State == WorkflowInstanceState.Initial)
            {
                await instance.StepAsync(token);
            }
        }
    }
}
