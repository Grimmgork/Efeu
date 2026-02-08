using Efeu.Runtime.Value;
using Efeu.Runtime.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Efeu.Runtime
{
    public enum EfeuBehaviourStepKind
    {
        Emit,
        Let,
        Await,
        If,
        Unless,
        For,
        On
    }

    public class EfeuBehaviourStep
    {
        public Guid Id;

        public EfeuBehaviourStepKind Kind;

        public string Name = "";

        public EfeuBehaviourExpression Input = EfeuBehaviourExpression.Empty;

        public EfeuBehaviourStep[] Do = [];

        public EfeuBehaviourStep[] Else = [];

        public EfeuBehaviourStep[] Error = [];
    }

    public enum EfeuExpressionType
    {
        Literal,
        Struct,
        Array,
        Script,
        Eval
    }

    public class EfeuBehaviourExpression
    {
        [JsonIgnore]
        public Func<EfeuRuntimeScope, EfeuValue> Func = (_) => default;

        public EfeuExpressionType Type;

        public EfeuValue Value;

        public string Code = "";

        public Dictionary<string, EfeuBehaviourExpression> Fields = [];

        public EfeuBehaviourExpression[] Items = [];


        public static EfeuBehaviourExpression Empty = new EfeuBehaviourExpression();

        public static EfeuBehaviourExpression Eval(Func<EfeuRuntimeScope, EfeuValue> func)
        {
            return new EfeuBehaviourExpression()
            {
                Type = EfeuExpressionType.Eval,
                Func = func
            };
        }

        public static EfeuBehaviourExpression Eval(Func<EfeuValue> func)
        {
            return new EfeuBehaviourExpression()
            {
                Type = EfeuExpressionType.Eval,
                Func = (_) => func()
            };
        }

        public static EfeuBehaviourExpression Eval(EfeuValue literal)
        {
            return new EfeuBehaviourExpression()
            {
                Type = EfeuExpressionType.Literal,
                Value = literal
            };
        }

        public static EfeuBehaviourExpression Script(string script)
        {
            return new EfeuBehaviourExpression()
            {
                Type = EfeuExpressionType.Script,
                Code = script,
            };
        }

        public EfeuValue Evaluate(EfeuRuntimeScope context)
        {
            return Type switch
            {
                EfeuExpressionType.Literal => Value,
                EfeuExpressionType.Eval => Func(context),
                EfeuExpressionType.Script => EfeuScript.Run(Code, new EfeuScriptScope(context)),
                EfeuExpressionType.Struct => new EfeuHash(Fields.Select(i =>
                    new KeyValuePair<string, EfeuValue>(i.Key, i.Value.Evaluate(context)))),
                EfeuExpressionType.Array => new EfeuArray(Items.Select(i => i.Evaluate(context))),
                _ => throw new Exception()
            };
        }
    }
}
