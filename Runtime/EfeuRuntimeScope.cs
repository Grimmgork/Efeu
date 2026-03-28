using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class EfeuRuntimeScope
    {
        public readonly ImmutableDictionary<string, EfeuValue> Constants = ImmutableDictionary<string, EfeuValue>.Empty;

        public static readonly EfeuRuntimeScope Empty = new EfeuRuntimeScope();

        private EfeuRuntimeScope() { }

        public EfeuRuntimeScope(ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Constants = constants;
        }

        public EfeuValue Get(string name)
        {
            return Constants[name];
        }

        public EfeuRuntimeScope With(string name, EfeuValue value)
        {
            return new EfeuRuntimeScope(Constants.SetItem(name, value));
        }
    }
}
