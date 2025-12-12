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

        [JsonIgnore]
        public Func<BehaviourExpressionContext, EfeuValue> Input = (_) => default;

        public BehaviourMessageMatch[] Where = [];

        public BehaviourDefinitionStep[] Do = [];

        public BehaviourDefinitionStep[] Else = [];
    }

    public class BehaviourMessageMatch
    {
        public string Field = "";

        public int Index = 0;

        [JsonIgnore]
        public EfeuExpression Expression = new EfeuExpression();

        public BehaviourMessageMatch[] Fields = [];
    }

    public class EfeuExpression
    {
        private readonly Func<BehaviourExpressionContext, EfeuValue> func;

        public EfeuExpression(Func<BehaviourExpressionContext, EfeuValue> func)
        {
            this.func = func;
        }

        public EfeuExpression()
        {
            this.func = (_) => default;
        }

        public EfeuValue Run(BehaviourExpressionContext context)
        {
            return func(context);
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
