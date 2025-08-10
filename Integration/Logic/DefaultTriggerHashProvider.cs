using Efeu.Runtime.Signal;
using Efeu.Runtime.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Logic
{
    internal class DefaultTriggerHashProvider : IWorkflowTriggerHashProvider
    {
        public WorkflowTrigger GetTriggerHash(object signalOrTrigger)
        {
            if (signalOrTrigger is IWorkflowTrigger hashProvider)
            {
                return hashProvider.GetTrigger();
            }
            else
            {
                return WorkflowTrigger.Signal(signalOrTrigger.GetType().Name);
            }
        }
    }
}
