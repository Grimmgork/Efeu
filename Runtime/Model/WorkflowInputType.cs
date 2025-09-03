using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public enum WorkflowInputType
    {
        Literal,
        Array,
        Struct,
        Output,
        Variable, // TODO
        Pipe, // TODO output of last Method (not applicable for functions)
    }
}
