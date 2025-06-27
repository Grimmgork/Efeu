using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Function;
using Workflows.Message;
using Workflows.Signal;

namespace Workflows
{
    public class ConsoleSignalHandler : IWorkflowSignalHandler
    {
        public Task RaiseSignal(WorkflowSignal message)
        {
            return Task.Run(() => Console.WriteLine($"SIGNAL: {message.GetType()}"));
        }

        public Task<WorkflowSignal> WaitForSignal(CancellationToken token)
        {
            string message = Console.ReadLine() ?? "";
            return Task.FromResult<WorkflowSignal>(new PromptInputSignal(message, DateTime.Now));
        }
    }
}
