using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class IfMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            if (!context.Input["If"].ToBoolean())
            {
                context.Route = "Else";
            }

            return WorkflowMethodState.Done;
        }
    }
}
