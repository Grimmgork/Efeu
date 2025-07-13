using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Function
{
    public class WorkflowValueContext
    {
        public readonly IReadOnlyDictionary<string, SomeData> Variables;

        public readonly IReadOnlyDictionary<int, SomeData> MethodOutputs;

        public readonly SomeData WorkflowInput;

        private Func<SomeData, SomeData> doFunc;

        public SomeData Do(SomeData input) => doFunc(input);

        public WorkflowValueContext(SomeData workflowInput, SomeStruct variables, IReadOnlyDictionary<int, SomeData> methodOutputs, Func<SomeData, SomeData> doFunc)
        {
            WorkflowInput = workflowInput;
            Variables = variables;
            MethodOutputs = methodOutputs;
            this.doFunc = doFunc;
        }
    }
}
