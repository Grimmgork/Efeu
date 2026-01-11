using Efeu.Runtime.Value;
using Efeu.Runtime.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Efeu.Runtime
{
    public enum BehaviourStepType
    {
        Emit,
        Let,
        Call,
        Await,
        If,
        Unless,
        For,
        On
    }

    public class BehaviourDefinitionStep
    {
        public Guid Id;

        public BehaviourStepType Type;

        public string Name = "";

        public EfeuExpression Input = EfeuExpression.Empty;

        public BehaviourDefinitionStep[] Do = [];

        public BehaviourDefinitionStep[] Else = [];
    }

    public enum EfeuExpressionType
    {
        Literal,
        Struct,
        Array,
        Script,
        Eval
    }

    public class EfeuExpression
    {
        private Func<EfeuExpressionContext, EfeuValue> func = (_) => default;

        public EfeuExpressionType Type;

        public EfeuValue Value;

        public string Code = "";

        public Dictionary<string, EfeuExpression> Fields = [];

        public EfeuExpression[] Items = [];

        public static EfeuExpression Empty = new EfeuExpression();

        public static EfeuExpression Eval(Func<EfeuExpressionContext, EfeuValue> func)
        {
            return new EfeuExpression()
            {
                Type = EfeuExpressionType.Eval,
                func = func
            };
        }

        public static EfeuExpression Eval(Func<EfeuValue> func)
        {
            return new EfeuExpression()
            {
                Type = EfeuExpressionType.Eval,
                func = (_) => func()
            };
        }

        public static EfeuExpression Eval(EfeuValue literal)
        {
            return new EfeuExpression()
            {
                Type = EfeuExpressionType.Literal,
                Value = literal
            };
        }

        public static EfeuExpression Script(string script)
        {
            return new EfeuExpression()
            {
                Type = EfeuExpressionType.Script,
                Code = script,
            };
        }

        public EfeuValue Evaluate(EfeuExpressionContext context)
        {
            return Type switch
            {
                EfeuExpressionType.Literal => Value,
                EfeuExpressionType.Eval => func(context),
                EfeuExpressionType.Script => EfeuScript.Run(Code, new EfeuScriptScope(context)),
                EfeuExpressionType.Struct => new EfeuHash(Fields.Select(i =>
                    new KeyValuePair<string, EfeuValue>(i.Key, i.Value.Evaluate(context)))),
                EfeuExpressionType.Array => new EfeuArray(Items.Select(i => i.Evaluate(context))),
                _ => throw new Exception()
            };
        }
    }
}
