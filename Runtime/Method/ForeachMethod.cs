using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class ForeachMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            if (context.InitialRun)
            {
                if (context.Input.IsNull)
                    return WorkflowMethodState.Done;

                if (context.Input.Items.Count == 0)
                    return WorkflowMethodState.Done;

                context.Data = SomeData.Struct([
                    new ("Items", context.Input),
                    new ("Index", 0)
                ]);
            }

            int index = context.Data["Index"].ToInt32();
            SomeData items = context.Data["Items"];
            SomeData item = items[index];
            if (index >= items.Items.Count)
                return WorkflowMethodState.Done;

            context.Output = item;
            index++;
            context.Data = SomeData.Struct([
                new ("Items", items),
                new ("Index", index)
            ]);
            return WorkflowMethodState.Dispatch;
        }
    }
}
