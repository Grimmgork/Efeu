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
        private Func<SomeData, SomeData> computeLambda;

        public WorkflowFunctionContext(Func<SomeData, SomeData> computeLambda)
        {
            this.computeLambda = computeLambda;
        }

        public SomeData ComputeLambda(SomeData input)
        {
            return computeLambda(input);
        }
    }
}
