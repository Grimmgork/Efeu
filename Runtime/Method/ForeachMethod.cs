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
        private class State
        {
            public IEnumerable<SomeData> Items = [];

            public List<SomeData> Result = [];

            public int Index;
        }

        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            State state = new State();
            if (context.IsFirstRun)
            {
                state = new State()
                {
                    Index = 0,
                    Items = context.Input.Items,
                    Result = []
                };

                context.Data = SomeData.Reference(state);

                if (state.Items.Any())
                {
                    context.Output = SomeData.Array();
                    return WorkflowMethodState.Done;
                }

                context.Output = state.Items.First();
                return WorkflowMethodState.Dispatch;
            }

            state.Result.Add(context.Result);

            if (state.Index < state.Items.Count())
            {
                context.Output = state.Items.ElementAt(state.Index);
                state.Index++;
                return WorkflowMethodState.Dispatch;
            }

            context.Output = SomeData.Array(state.Result);
            return WorkflowMethodState.Done;
        }
    }
}
