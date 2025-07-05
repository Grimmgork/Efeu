using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class WhileMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            if (context.Input["Condition"].ToBoolean())
            {
                return WorkflowMethodState.Dispatch;
            }

            return WorkflowMethodState.Done;
        }
    }
}
