using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class BehaviourScopeEntity
    {
        public Guid Id { get; set; }

        public uint ReferenceCount { get; set; }

        public ImmutableDictionary<string, EfeuValue> Constants { get; set; } = ImmutableDictionary<string, EfeuValue>.Empty;
    }
}
