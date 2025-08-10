using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    internal class ChronTrigger : WorkflowTrigger<ChronRaisedSignal>
    {
        public string Chron;
    }
}
