using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Workflows.Function;

namespace Workflows.Data
{
    public static class InputSource
    {
        public static IInputSource MethodOutput(int id, SomeDataTraversal name)
        {
            return new MethodOutput()
            {
                Id = id,
                Name = name,
            };
        }

        public static IInputSource FunctionOutput(int id, SomeDataTraversal name)
        {
            return new FunctionOutput()
            {
                Id = id,
                Name = name,
            };
        }

        public static IInputSource Variable(SomeDataTraversal name)
        {
            return new Variable()
            {
                Name = name
            };
        }

        public static IInputSource Input(SomeDataTraversal traversal)
        {
            return new WorkflowInput(traversal);
        }

        public static IInputSource Literal(SomeData literal)
        {
            return new Literal()
            {
                Value = literal
            };
        }

        public static IInputSource Func(Func<InputEvaluationContext, SomeData> func)
        {
            return new CSharpFunc()
            {
                Expression = func
            };
        }
    }


    public class InputEvaluationContext
    {
        public readonly ISomeTraversableData Variables;

        public readonly ISomeTraversableData WorkflowInput;

        public readonly Func<int, SomeDataTraversal, SomeData> GetFunctionOutput;

        public readonly Func<int, SomeDataTraversal, SomeData> GetMethodOutput;

        public InputEvaluationContext(ISomeTraversableData variables, ISomeTraversableData workflowInput, Func<int, SomeDataTraversal, SomeData> getMethodOutput, Func<int, SomeDataTraversal, SomeData> getFunctionOutput)
        {
            Variables = variables;
            WorkflowInput = workflowInput;
            GetMethodOutput = getMethodOutput;
            GetFunctionOutput = getFunctionOutput;
        }
    }


    public class Literal : IInputSource
    {
        public SomeData Value { get; set; }

        public SomeData GetValue(InputEvaluationContext context)
        {
            return Value;
        }
    }

    public class Variable : IInputSource
    {
        public SomeDataTraversal Name { get; set; }

        public SomeData GetValue(InputEvaluationContext context)
        {
            return context.Variables.Traverse(Name);
        }
    }

    public class MethodOutput : IInputSource
    {
        public int Id;

        public SomeDataTraversal Name;

        public SomeData GetValue(InputEvaluationContext context)
        {
            return context.GetMethodOutput(Id, Name);
        }
    }

    public class FunctionOutput : IInputSource
    {
        public int Id;

        public SomeDataTraversal Name;

        public SomeData GetValue(InputEvaluationContext context)
        {
            return context.GetFunctionOutput(Id, Name);   
        }
    }

    // cannot be persisted?
    public class CSharpFunc : IInputSource
    {
        public Func<InputEvaluationContext, SomeData> Expression = (context) => SomeData.Undef();

        public SomeData GetValue(InputEvaluationContext context)
        {
            return Expression(context);
        }
    }

    public interface IInputSource
    {
        public SomeData GetValue(InputEvaluationContext context);
    }
}
