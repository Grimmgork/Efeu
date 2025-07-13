using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Method
{
    public class PrintMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            Console.WriteLine(context.Input.ToString());
            return WorkflowMethodState.Done;
        }
    }
}
