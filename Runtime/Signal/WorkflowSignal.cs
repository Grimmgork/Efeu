using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    internal class WorkflowSignal
    {
        public int Name { get; set; }
        public SomeData Payload { get; set; }
    }
}
