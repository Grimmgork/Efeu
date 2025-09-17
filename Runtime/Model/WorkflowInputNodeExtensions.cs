using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public static class WorkflowInputNodeExtensions
    {
        public static EfeuValue EvaluateInput(this WorkflowInputNode node, InputEvaluationContext context)
        {
            if (node.Type == WorkflowInputType.Literal)
            {
                return node.Value;
            }
            else
            if (node.Type == WorkflowInputType.Array)
            {
                return new EfeuArray(node.Inputs.Select(i => i.EvaluateInput(context)));
            }
            else
            if (node.Type == WorkflowInputType.Struct)
            {
                return new EfeuHash(node.Inputs.Select(i => new KeyValuePair<string, EfeuValue>(i.Name, i.EvaluateInput(context))));
            }
            else
            if (node.Type == WorkflowInputType.Output)
            {
                return context.GetOutput(node.ActionId).Traverse(node.Traversal);
            }
            else
            if (node.Type == WorkflowInputType.Pipe)
            {
                return context.LastOutput;
            }
            else
            if (node.Type == WorkflowInputType.Variable)
            {
                // TODO
            }

            throw new NotImplementedException();
        }
    }
}
