using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Efeu.Runtime.Function;
using Efeu.Runtime.Model;

namespace Efeu.Runtime.Data
{
    public class InputEvaluationContext
    {
        public readonly ISomeTraversableData Variables;

        public readonly ISomeTraversableData WorkflowInput;

        public readonly Func<int, SomeData> GetOutput;

        public InputEvaluationContext(ISomeTraversableData variables, ISomeTraversableData workflowInput, Func<int, SomeData> getOutput)
        {
            Variables = variables;
            WorkflowInput = workflowInput;
            GetOutput = getOutput;
        }
    }
}
