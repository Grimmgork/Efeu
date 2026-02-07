using Efeu.Runtime.Value;
using Efeu.Runtime.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Efeu.Runtime
{
    public enum BehaviourStepKind
    {
        Emit,
        Let,
        Await,
        If,
        Unless,
        For,
        On
    }

    public class BehaviourDefinitionStep
    {
        public Guid Id;

        public BehaviourStepKind Kind;

        public string Name = "";

        public BehaviourDefinitionExpression Input = BehaviourDefinitionExpression.Empty;

        public BehaviourDefinitionStep[] Do = [];

        public BehaviourDefinitionStep[] Else = [];

        public BehaviourDefinitionStep[] Error = [];
    }

    public enum EfeuExpressionType
    {
        Literal,
        Struct,
        Array,
        Script,
        Eval
    }

    public class BehaviourDefinitionExpression
    {
        [JsonIgnore]
        public Func<EfeuRuntimeScope, EfeuValue> Func = (_) => default;

        public EfeuExpressionType Type;

        public EfeuValue Value;

        public string Code = "";

        public Dictionary<string, BehaviourDefinitionExpression> Fields = [];

        public BehaviourDefinitionExpression[] Items = [];


        public static BehaviourDefinitionExpression Empty = new BehaviourDefinitionExpression();

        public static BehaviourDefinitionExpression Eval(Func<EfeuRuntimeScope, EfeuValue> func)
        {
            return new BehaviourDefinitionExpression()
            {
                Type = EfeuExpressionType.Eval,
                Func = func
            };
        }

        public static BehaviourDefinitionExpression Eval(Func<EfeuValue> func)
        {
            return new BehaviourDefinitionExpression()
            {
                Type = EfeuExpressionType.Eval,
                Func = (_) => func()
            };
        }

        public static BehaviourDefinitionExpression Eval(EfeuValue literal)
        {
            return new BehaviourDefinitionExpression()
            {
                Type = EfeuExpressionType.Literal,
                Value = literal
            };
        }

        public static BehaviourDefinitionExpression Script(string script)
        {
            return new BehaviourDefinitionExpression()
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
