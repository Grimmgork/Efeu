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
    public class ConsoleSignalHandler : IWorkflowSignalHandler
    {
        public Task RaiseSignal(CustomWorkflowSignal message)
        {
            return Task.Run(() => Console.WriteLine($"SIGNAL: {message.GetType()}"));
        }

        public Task<CustomWorkflowSignal> WaitForSignal(CancellationToken token)
        {
            string message = Console.ReadLine() ?? "";
            // return Task.FromResult<CustomWorkflowSignal>(new PromptInputSignal(message, DateTime.Now));

            throw new Exception();
        }
    }
}
