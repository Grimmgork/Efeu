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
        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            if (context.IsFirstRun)
            {
                if (context.Input.As<EfeuArray>().Count() == 0)
                {
                    context.Output = new EfeuArray();
                    return WorkflowMethodState.Done;
                }

                context.Data = new EfeuHash([
                    new ("result", new EfeuArray()),
                    new ("index", 0)
                ]);

                context.Output = context.Input.First();
                return WorkflowMethodState.Yield;
            }

            if (context.Result)
            {
                int index = context.Data.Call("index").ToInt();
                EfeuValue item = context.Input.Call(index);
                context.Data = context.Data.Call("result").Push(item);
            }

            context.Data.Call("index", (value) => value++);
            if (context.Data.Call("index") < context.Input.Length())
            {
                int index = context.Data.Call("index").ToInt();
                context.Output = context.Input.Call(index);
                return WorkflowMethodState.Yield;
            }

            context.Output = context.Data.Call("result");
            return WorkflowMethodState.Done;
        }
    }
}
