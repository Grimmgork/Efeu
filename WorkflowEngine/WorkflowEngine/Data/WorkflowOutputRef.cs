using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Workflows.Data
{
    public struct WorkflowOutputRef
    {
        public readonly int Id;
        public readonly string Name;

        public WorkflowOutputRef(int methodId, string name)
        {
            Id = methodId;
            Name = name;
        }
    }
}
