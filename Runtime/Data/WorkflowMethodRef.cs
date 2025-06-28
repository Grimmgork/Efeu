using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public struct WorkflowMethodRef
    {
        public readonly int Id;

        public WorkflowMethodRef(int id)
        {
            Id = id;
        }

        public static implicit operator int(WorkflowMethodRef d) => d.Id;
        public static implicit operator WorkflowMethodRef(int d) => new WorkflowMethodRef(d);
    }
}
