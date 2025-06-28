using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Function
{
    public class WorkflowFunctionContext
    {
        public readonly IReadOnlyDictionary<string, SomeData> Variables;

        public WorkflowFunctionContext(SomeDataStruct variables)
        {
            Variables = variables;
        }
    }
}
