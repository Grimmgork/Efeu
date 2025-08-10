using Efeu.Runtime.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    internal class WorkflowTrigger<T> where T : WorkflowSignal
    {
        public virtual string GetSignalHash(T signal)
        {
            return typeof(T).Name;
        }

        public virtual string GetTriggerHash()
        {
            return typeof(T).Name;
        }
    }
}
