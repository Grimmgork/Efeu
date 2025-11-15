using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Script;

namespace Efeu.Runtime.Function
{
    public class WorkflowFunction : WorkflowFunctionInstanceBase
    {
        private Func<WorkflowFunctionContext, EfeuValue, EfeuValue> run;

        public WorkflowFunction(Func<EfeuValue, EfeuValue> run)
        {
            this.run = (context, input) => run(input);
        }

        public WorkflowFunction(Func<WorkflowFunctionContext, EfeuValue, EfeuValue> run)
        {
            this.run = run;
        }

        public sealed override EfeuValue Run(WorkflowFunctionContext context, EfeuValue inputs)
        {
            return run.Invoke(context, inputs);
        }
    }
}
