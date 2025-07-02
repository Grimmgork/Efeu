using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime.Method
{
    public class ForMethod : WorkflowMethodBase
    {
        public override WorkflowMethodState Run(WorkflowMethodContext context, CancellationToken token)
        {
            int from = context.Input["From"].ToInt32();
            int to = context.Input["To"].ToInt32();

            if (context.FirstRun)
            {
                if (to - from <= 0)
                {
                    return WorkflowMethodState.Done;
                }

                context.Data = SomeData.Struct();
                context.Data["Counter"] = from;
            }

            int counter = context.Data["Counter"].ToInt32();
            if (counter >= to)
            {
                return WorkflowMethodState.Done;
            }

            counter++;
            context.Data["Counter"] = counter;
            context.Output["Count"] = counter;
            return WorkflowMethodState.Dispatch;
        }
    }
}
