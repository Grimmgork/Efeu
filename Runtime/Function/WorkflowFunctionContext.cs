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
        private Func<SomeData, SomeData> doFunc;

        public WorkflowFunctionContext(Func<SomeData, SomeData> doFunc)
        {
            this.doFunc = doFunc;
        }

        public SomeData Do(SomeData input) => doFunc(input);
    }
}
