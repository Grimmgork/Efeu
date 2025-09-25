using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;
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
        public EfeuValue Input;
        public WorkflowTriggerHash Hash;
    }
}
