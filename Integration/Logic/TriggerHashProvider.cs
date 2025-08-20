using Efeu.Runtime.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Logic
{
    internal class TriggerHashProvider : IWorkflowTriggerHashProvider
    {
        public string GetTriggerHash(object signal)
        {
            if (signal is IWorkflowSignal hashProvider)
            {
                return hashProvider.GetTriggerHash();
            }
            else
            {
                return signal.GetType().Name;
            }
        }
    }
}
