using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Function
{
    public interface IWorkflowFunctionInstance
    {
        public SomeData Run(WorkflowFunctionContext context, SomeStruct input);
    }
}
