using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Method;
using Efeu.Runtime.Script;

namespace Efeu.Runtime.Function
{
    public abstract class WorkflowFunctionInstanceBase : IWorkflowFunction
    {
        public virtual EfeuValue Run(WorkflowFunctionContext context, EfeuValue input)
        {
            throw new NotImplementedException();
        }
    }
}
