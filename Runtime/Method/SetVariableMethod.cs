using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class SetVariableMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            string name = context.Input.Properties["Name"].ToString();
            context.Variables[name] = context.Input.Properties["Value"];
            return WorkflowMethodState.Done;
        }
    }
}
