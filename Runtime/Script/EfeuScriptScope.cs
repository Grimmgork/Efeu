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
        public readonly ImmutableDictionary<string, EfeuValue> Constants;

        public static EfeuScriptScope Empty { get; } = new EfeuScriptScope();

        public EfeuScriptScope()
        {
            Constants = ImmutableDictionary<string, EfeuValue>.Empty;
        }

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
            return this.Constants[name];
        }

        public EfeuScriptScope With(string name, EfeuValue value)
        {
            return new EfeuScriptScope(Constants.SetItem(name, value));
        }
    }
}
