using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public static class WorkflowInputNodeExtensions
    {
        public static SomeData EvaluateInput(this WorkflowInputNode node, InputEvaluationContext context)
        {
            if (node.Type == WorkflowInputType.Literal)
            {
                return node.Value;
            }
            else
            if (node.Type == WorkflowInputType.Array)
            {
                return SomeData.Array(node.Inputs.Select(i => i.EvaluateInput(context)));
            }
            else
            if (node.Type == WorkflowInputType.Struct)
            {
                return SomeData.Struct(node.Inputs.Select(i => new KeyValuePair<string, SomeData>(i.Name, i.EvaluateInput(context))));
            }
            else
            if (node.Type == WorkflowInputType.Output)
            {
                return context.GetOutput(node.ActionId).Traverse(node.Traversal);
            }
            else
            if (node.Type == WorkflowInputType.Variable)
            {
                return context.Variables.Traverse(node.Traversal);
            }

            throw new NotImplementedException();
        }
    }
}
