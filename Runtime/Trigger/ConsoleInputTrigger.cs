using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    /// <summary>
    /// Defines a trigger for <see cref="ConsoleInputSignal"/> payload.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConsoleInputTrigger : IWorkflowTrigger
    {
        public int CallbackId;

        public WorkflowTrigger GetTrigger()
        {
            return WorkflowTrigger.Signal(nameof(ConsoleInputSignal), CallbackId.ToString());
        }
    }
}
