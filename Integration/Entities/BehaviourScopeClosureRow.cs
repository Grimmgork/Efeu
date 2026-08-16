using System;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Entities;

public class BehaviourScopeClosureRow
{
    public Guid ScopeId;

    public string? Constant;

    public int Sequence;

    public EfeuValueReference Value;
}