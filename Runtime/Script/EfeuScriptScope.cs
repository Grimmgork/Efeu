using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Script
{

    public interface IEfeuScope
    {
        public EfeuValue Get(string name);
    }

    public class EfeuScriptScope : IEfeuScope
    {
        private readonly IEfeuScope? parent;

        private readonly ImmutableDictionary<string, EfeuValue> constants;

        public static EfeuScriptScope Empty = new EfeuScriptScope();

        public EfeuScriptScope()
        {
            this.constants = ImmutableDictionary<string, EfeuValue>.Empty;
        }

        public EfeuScriptScope(IEfeuScope scope)
        {
            parent = scope;
            constants = ImmutableDictionary<string, EfeuValue>.Empty;
        }

        public EfeuScriptScope(IEnumerable<KeyValuePair<string, EfeuValue>> constants)
        {
            this.constants = ImmutableDictionary<string, EfeuValue>.Empty.AddRange(constants);
        }

        public EfeuScriptScope(ImmutableDictionary<string, EfeuValue> constants)
        {
            this.constants = constants;
        }

        private EfeuScriptScope(IEfeuScope parent, ImmutableDictionary<string, EfeuValue> constants)
        {
            this.parent = parent;
            this.constants = constants;
        }

        private EfeuScriptScope(IEfeuScope parent, IEnumerable<KeyValuePair<string, EfeuValue>> constants)
        {
            this.parent = parent;
            this.constants = ImmutableDictionary<string, EfeuValue>.Empty.AddRange(constants);
        }

        public EfeuValue Get(string name)
        {
            if (this.constants.TryGetValue(name, out EfeuValue value))
            {
                return value;
            }
            else
            {
                return parent?.Get(name) ?? default;
            }
        }

        public EfeuScriptScope Push(string name, EfeuValue value)
        {
            return this.Push([new(name, value)]);
        }

        public EfeuScriptScope Push(ImmutableDictionary<string, EfeuValue> constants)
        {
            return new EfeuScriptScope(this, constants);
        }

        public EfeuScriptScope Push(IEnumerable<KeyValuePair<string, EfeuValue>> constants)
        {
            return new EfeuScriptScope(this, constants);
        }
    }
}
