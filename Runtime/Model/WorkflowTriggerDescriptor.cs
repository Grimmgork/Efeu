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
        public Type Type;
        public string Hash;
        public int WorkflowInstanceId;
        public int WorkflowDefinitionVersionId;
        public SomeData Data;
    }
}
