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
            string outputName = context.Input["Name"].ToString();
            SomeData value = context.Input["Value"];
            context.WorkflowOutput[outputName] = value;
            return WorkflowMethodState.Done;
        }
    }
}
