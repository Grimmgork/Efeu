using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Efeu.Router
{
    public class BehaviourDefinition
    {
        public string Id = "";

        public string Name = "";

        public int Version;

        public BehaviourDefinitionStep[] Steps = [];
    }

    public class BehaviourDefinitionStep
    {
        public BehaviourStepType Type;

        public string Name = "";

        // public EfeuValue Expression;

        public Func<BehaviourScope, EfeuValue> Expression = (_) => default;

        public Dictionary<string, EfeuValue> Match = new Dictionary<string, EfeuValue>();

        public BehaviourDefinitionStep[] Do = [];

        public BehaviourDefinitionStep[] Else = [];
    }



    public enum BehaviourStepType
    {
        On,
        Let,
        Emit,
        Call,
        Await,
        If,
        For
    }
}
