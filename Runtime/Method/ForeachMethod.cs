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
        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            if (context.IsFirstRun)
            {
                context.Data = new EfeuArray();

                if (context.Input.Length() == 0)
                {
                    context.Output = new EfeuArray();
                    return WorkflowMethodState.Done;
                }

                context.Output = context.Input.First();
                return WorkflowMethodState.Yield;
            }

            EfeuValue result = context.Data.Call("result");

            context.Data = result.Push(context.Result);
            if (context.Times < context.Input.Length())
            {
                context.Output = context.Input.Call(context.Times);
                return WorkflowMethodState.Yield;
            }

            context.Output = result;
            return WorkflowMethodState.Done;
        }
    }

}
