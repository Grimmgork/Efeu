using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class EvalMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            context.Output = context.Input;
            return WorkflowMethodState.Done;
        }
    }
}
