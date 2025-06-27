using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;

namespace Workflows.Function
{
    public class WorkflowFunction : WorkflowFunctionInstanceBase
    {
        private Func<SomeDataStruct, SomeDataStruct> run;

        public WorkflowFunction(Func<SomeDataStruct, SomeData> run)
        {
            this.run = (input) =>
            {
                SomeDataStruct output = new SomeDataStruct();
                output["Result"] = run(input);
                return output;
            };
        }

        public sealed override SomeDataStruct Run(IWorkflowSignalHandler context, SomeDataStruct inputs)
        {
            return run.Invoke(inputs);
        }
    }
}
