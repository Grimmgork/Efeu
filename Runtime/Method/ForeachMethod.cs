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
                IList<SomeData> items = context.Input["Items"].Items;
                if (!items.Any())
                    return WorkflowMethodState.Done;

                context.Data = SomeData.Struct(); 
                context.Data["Index"] = 0;
                context.Output["Item"] = items.First();
                return WorkflowMethodState.Dispatch;
            }
            else
            {
                int index = context.Data["Index"].ToInt32();
                if (index > items.Count)
                    return WorkflowMethodState.Done;

                context.Output["Item"] = items[index];
                return WorkflowMethodState.Dispatch;
            }
        }
    }
}
