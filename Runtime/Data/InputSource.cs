using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Efeu.Runtime.Function;

namespace Efeu.Runtime.Data
{
    public static class InputSource
    {
        public static IInputSource MethodOutput(int id, SomeDataTraversal name = default)
        {
            return new MethodOutput()
            {
                Id = id,
                Name = name,
            };
        }

        public static IInputSource FunctionOutput(int id, SomeDataTraversal name = default)
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

        public static IInputSource Input(SomeDataTraversal traversal = default)
        {
            return new WorkflowInput()
            {
                Name = traversal
            };
        }

        public static IInputSource Literal(SomeData literal)
        {
            return new Literal()
            {
                Value = literal
            };
        }

        public static IInputSource LambdaInput(SomeDataTraversal traversal = default)
        {
            return new LambdaInput()
            {
                Name = traversal
            };
        }

        public static IInputSource Lambda(Func<InputEvaluationContext, SomeData> func)
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

        public readonly SomeData LambdaInput;

        public InputEvaluationContext(ISomeTraversableData variables, ISomeTraversableData workflowInput, Func<int, SomeDataTraversal, SomeData> getMethodOutput, Func<int, SomeDataTraversal, SomeData> getFunctionOutput, SomeData lambdaInput)
        {
            Variables = variables;
            WorkflowInput = workflowInput;
            GetMethodOutput = getMethodOutput;
            GetFunctionOutput = getFunctionOutput;
            LambdaInput = lambdaInput;
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

    public class LambdaInput : IInputSource
    {
        public SomeDataTraversal Name { get; set; }

        public SomeData GetValue(InputEvaluationContext context)
        {
            return context.LambdaInput.Traverse(Name);
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

    public class CSharpFunc : IInputSource
    {
        public Func<InputEvaluationContext, SomeData> Expression = (context) => SomeData.Null();

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
