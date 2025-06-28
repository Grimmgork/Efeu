using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
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
