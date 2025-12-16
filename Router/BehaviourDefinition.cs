using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Efeu.Router
{
    public class BehaviourDefinitionStep
    {
        public BehaviourStepType Type;

        public string Name = "";

        public EfeuValue Literal;

        [JsonIgnore]
        public Func<BehaviourExpressionContext, EfeuValue> Expression = (_) => default;

        public BehaviourMessageMatch[] Where = [];

        public BehaviourDefinitionStep[] Do = [];

        public BehaviourDefinitionStep[] Else = [];
    }

    public class BehaviourMessageMatch
    {
        public string Field = "";

        public int Index = 0;

        public string Script = "";

        [JsonIgnore]
        public Func<BehaviourExpressionContext, EfeuValue> Func = (_) => default;

        public EfeuValue Literal;

        public BehaviourMessageMatch[] Where = [];
    }

    public class EfeuExpression
    {
        private readonly Func<BehaviourExpressionContext, EfeuValue> func = (_) => default;

        private readonly string script = "";

        public bool IsEmpty => string.IsNullOrEmpty(script);

        public EfeuExpression()
        {
            
        }

        public EfeuExpression(Func<BehaviourExpressionContext, EfeuValue> func)
        {
            this.func = func;
        }

        public EfeuExpression(string script)
        {
            this.script = script;
        }

        public static implicit operator EfeuExpression(string script) => new EfeuExpression(script);

        public static implicit operator EfeuExpression(Func<BehaviourExpressionContext, EfeuValue> func) => new EfeuExpression(func);

        public static implicit operator EfeuExpression(Func<EfeuValue> func) => new EfeuExpression((_) => func());

        public EfeuValue Evaluate(BehaviourExpressionContext context)
        {
            if (string.IsNullOrEmpty(script))
                return func(context);
            else
                return script;
        }
    }

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
}
