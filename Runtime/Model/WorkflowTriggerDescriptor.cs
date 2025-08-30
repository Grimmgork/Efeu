using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public class WorkflowTriggerDescriptor
    {
        public int Id;
        public SomeData Input;
        public SomeData Data;
        public object Signal = new object();
    }
}
