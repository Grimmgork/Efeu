using System;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Entities;

public class ValueNodeClosureEntity
{
    public int Id;

    public DateTimeOffset Created;

    public EfeuValueHash ValueHash;
}

public class ValueNodeClosureRow
{
    public int ClosureId;
    
    public int Sequence;
    
    public EfeuValueHash ValueHash;
}