using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;

namespace Workflows.Method
{
    public class PrintMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            Console.WriteLine(context.Input["Message"].ToString());
            return WorkflowMethodState.Done;
        }
    }
}
