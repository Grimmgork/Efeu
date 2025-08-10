using Efeu.Integration.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    public class ConsoleInputSignal : IWorkflowTrigger
    {
        public int CallbackId;

        public readonly string Input;

        public ConsoleInputSignal(string input)
        {
            Input = input;
        }

        public WorkflowTrigger GetTrigger()
        {
            return WorkflowTrigger.Signal(nameof(ConsoleInputSignal), CallbackId.ToString());
        }
    }
}
