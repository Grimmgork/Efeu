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
    /// Defines a trigger for a specific signal <typeparamref name="T"/> payload.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SpecificTrigger : WorkflowTrigger<RequestInputSignal>
    {
        public int SomeId;

        public override string GetSignalHash(RequestInputSignal signal)
        {
            return nameof(WorkflowSignal) + "|" + signal.Payload.ToString();
        }

        public override string GetTriggerHash()
        {
            return nameof(WorkflowSignal) + "|" + SomeId.ToString();
        }
    }
}
