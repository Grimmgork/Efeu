using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    public abstract class WorkflowSignal
    {
        public readonly string Name;

        public readonly DateTime Timestamp;

        public WorkflowSignal(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }
    }
}
