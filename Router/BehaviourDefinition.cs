using Efeu.Runtime.Data;
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
        public Func<BehaviourExpressionContext, EfeuValue> Value = (_) => default;

        public BehaviourMessageMatch[] Fields = [];
    }

    public enum BehaviourStepType
    {
        On,
        Let,
        Emit,
        Call,
        Await,
        If,
        Unless,
        For
    }
}
