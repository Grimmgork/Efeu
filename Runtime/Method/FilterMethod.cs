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
        private class State
        {
            public IReadOnlyCollection<SomeData> Items = [];

            public ICollection<SomeData> Result = [];

            public int Index;
        }

        public override WorkflowMethodState Run(WorkflowMethodContext context)
        {
            if (context.IsFirstRun)
            {
                context.Data = SomeData.Reference(new State()
                {
                    Index = 0,
                    Items = context.Input.Items,
                    Result = []
                });

                if (context.Input.Items.Count == 0)
                {
                    context.Output = SomeData.Array();
                    return WorkflowMethodState.Done;
                }

                context.Output = context.Input.Items.First();
                return WorkflowMethodState.Dispatch;
            }

            State state = (State)context.Data.Value!;
            if (context.Result.ToBoolean())
            {
                state.Result.Add(state.Items.ElementAt(state.Index));
            }

            state.Index++;
            if (state.Index < state.Items.Count)
            {
                context.Output = state.Items.ElementAt(state.Index);
                return WorkflowMethodState.Dispatch;
            }

            context.Output = SomeData.Array(state.Items);
            return WorkflowMethodState.Done;
        }
    }
}
