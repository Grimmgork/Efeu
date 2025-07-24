using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class FilterMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            if (context.InitialRun)
            {
                context.Data = SomeData.Struct([
                    new ("Items", context.Input),
                    new ("Result", SomeData.Array()),
                    new ("Index", 0)
                ]);
            }

            int index = context.Data["Index"].ToInt32();
            IReadOnlyCollection<SomeData> items = context.Data["Items"].Items;
            List<SomeData> result = context.Data["Result"].Items.ToList();

            if (!context.InitialRun && context.DispatchResult.ToBoolean())
            {
                result.Add(items.ElementAt(index-1));
            }

            if (index < items.Count)
            {
                context.Output = items.ElementAt(index);
                context.Data = SomeData.Struct([
                    new ("Items", context.Data["Items"]),
                    new ("Result", SomeData.Array(result)),
                    new ("Index", index+1),
                ]);

                return WorkflowMethodState.Dispatch;
            }
            else
            {
                context.Output = SomeData.Array(result);
                return WorkflowMethodState.Done;
            }
        }
    }
}
