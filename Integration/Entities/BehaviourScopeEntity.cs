using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities;

public class BehaviourScopeEntity
{
    public Guid Id;

    public uint ReferenceCount;

    public Guid LoopbackScopeId;

    public string LoopbackPosition = "";

    public ImmutableDictionary<string, EfeuValue> Constants = ImmutableDictionary<string, EfeuValue>.Empty;
}
