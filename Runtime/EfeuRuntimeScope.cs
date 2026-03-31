using Efeu.Runtime.Value;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Runtime
{
    public class EfeuRuntimeScope
    {
        public readonly Guid Id;

        public readonly ImmutableDictionary<string, EfeuValue> Constants;

        public static readonly EfeuRuntimeScope Empty = new EfeuRuntimeScope(Guid.Empty, ImmutableDictionary<string, EfeuValue>.Empty);

        public EfeuRuntimeScope(Guid id, ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Id = id;
            this.Constants = constants;
        }

        public EfeuValue Get(string name)
        {
            return Constants[name];
        }

        public EfeuRuntimeScope With(string name, EfeuValue value)
        {
            return new EfeuRuntimeScope(Guid.NewGuid(), Constants.SetItem(name, value));
        }
    }
}
