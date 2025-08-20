using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Function
{
    public interface IWorkflowFunction
    {
        public SomeData Run(WorkflowFunctionContext context, SomeData input);
    }
}
