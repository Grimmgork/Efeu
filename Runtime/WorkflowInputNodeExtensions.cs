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
            if (node.Type == WorkflowInputType.CSharp)
            {
                return node.CSharp(context);
            }
            else 
            if (node.Type == WorkflowInputType.Literal)
            {
                return node.Literal;
            }
            else
            if (node.Type == WorkflowInputType.Variable)
            {
                return context.Variables.Traverse(node.Traversal);
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
            if (node.Type == WorkflowInputType.MethodOutput)
            {
                return context.GetMethodOutput(node.Id, node.Traversal);
            }
            else
            if (node.Type == WorkflowInputType.FunctionOutput)
            {
                return context.GetFunctionOutput(node.Id, node.Traversal);
            }
            else
            if (node.Type == WorkflowInputType.DoInput)
            {
                return context.DoInput;
            }

            throw new NotImplementedException();
        }
    }
}
