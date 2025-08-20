using Efeu.Runtime.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    internal class ChronElapsedSignal : IWorkflowSignal
    {
        public Guid Id;

        public string GetTriggerHash()
        {
             return Id.ToString();
        }
    }
}
