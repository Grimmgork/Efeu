using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    internal class ChronElapsedSignal : IWorkflowSignal
    {
        public string Id = "";

        public DateTimeOffset Time;

        public string GetTriggerHash()
        {
             return $"{nameof(ChronElapsedSignal)}#{Id.ToString()}";
        }
    }
}
