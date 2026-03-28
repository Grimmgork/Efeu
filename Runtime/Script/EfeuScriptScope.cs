using Efeu.Runtime.Value;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Script
{
    public class EfeuScriptScope
    {
        public readonly ImmutableDictionary<string, EfeuValue> Constants = ImmutableDictionary<string, EfeuValue>.Empty;

        public static readonly EfeuScriptScope Empty = new EfeuScriptScope();

        private EfeuScriptScope() { }

        public EfeuScriptScope(ImmutableDictionary<string, EfeuValue> scope)
        {
            this.Constants = scope;
        }

        public EfeuScriptScope(EfeuRuntimeScope scope)
        {
            this.Constants = scope.Constants;
        }

        public EfeuValue Get(string name)
        {
            EfeuValue result = EfeuValue.Nil();
            this.Constants.TryGetValue(name, out result);
            return result;
        }

        public EfeuScriptScope With(string name, EfeuValue value)
        {
            return new EfeuScriptScope(Constants.SetItem(name, value));
        }
    }
}
