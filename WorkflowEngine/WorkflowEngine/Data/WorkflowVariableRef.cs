using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflows.Data
{
    public struct WorkflowVariableRef
    {
        public readonly string Name;

        public WorkflowVariableRef(string name)
        {
            Name = name;
        }
    }
}
