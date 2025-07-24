using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public enum WorkflowInputType
    {
        Literal,
        DispatchContext,
        MethodOutput,
        FunctionOutput,
        Variable,
        Array,
        Struct
    }
}
