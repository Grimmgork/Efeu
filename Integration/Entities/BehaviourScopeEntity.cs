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

        public Guid LoopbackScopeId { get; set; }

        public string LoopbackPosition { get; set; } = "";

        public EfeuArray LoopbackIterator { get; set; } = EfeuArray.Empty;

        public ImmutableDictionary<string, EfeuValue> Constants { get; set; } = ImmutableDictionary<string, EfeuValue>.Empty;
    }
}
