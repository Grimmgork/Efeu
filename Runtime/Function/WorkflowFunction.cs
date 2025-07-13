using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Function
{
    public class WorkflowFunction : WorkflowFunctionInstanceBase
    {
        private Func<WorkflowFunctionContext, SomeData, SomeData> run;

        public WorkflowFunction(Func<SomeData, SomeData> run)
        {
            this.run = (context, input) => run(input);
        }

        public WorkflowFunction(Func<WorkflowFunctionContext, SomeData, SomeData> run)
        {
            this.run = run;
        }

        public sealed override SomeData Run(WorkflowFunctionContext context, SomeData inputs)
        {
            return run.Invoke(context, inputs);
        }
    }
}
