using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Efeu.Runtime.Function;

namespace Efeu.Runtime.Data
{
    public class InputEvaluationContext
    {
        public readonly ISomeTraversableData Variables;

        public readonly ISomeTraversableData WorkflowInput;

        public readonly Func<int, SomeDataTraversal, SomeData> GetFunctionOutput;

        public readonly Func<int, SomeDataTraversal, SomeData> GetMethodOutput;

        public InputEvaluationContext(ISomeTraversableData variables, ISomeTraversableData workflowInput, Func<int, SomeDataTraversal, SomeData> getMethodOutput, Func<int, SomeDataTraversal, SomeData> getFunctionOutput)
        {
            Variables = variables;
            WorkflowInput = workflowInput;
            GetMethodOutput = getMethodOutput;
            GetFunctionOutput = getFunctionOutput;
        }
    }
}
