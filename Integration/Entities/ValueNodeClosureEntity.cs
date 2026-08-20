using System;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Entities;

public class ValueNodeClosureEntity
{
    public int Id;

    public DateTimeOffset Created;

    public EfeuReference Reference;
}

public class ValueNodeClosureRow
{
    public int ClosureId;
    
    public int Sequence;
    
    public EfeuReference Reference;
}