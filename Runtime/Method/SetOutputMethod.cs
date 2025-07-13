using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Method
{
    public class SetOutputMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            context.WorkflowOutput = context.Input;
            return WorkflowMethodState.Done;
        }
    }
}
