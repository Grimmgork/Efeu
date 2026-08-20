using System.Collections.Generic;
using Efeu.Integration.Entities;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Utils;

public class EfeuValueDeserializer
{
    public void Deserialize(EfeuReference reference, ValueNodeRow[] nodes)
    {
        
    }
}

public class EfeuValueSerializer
{
    public List<ValueNodeRow> Nodes = [];
    public ValueNodeClosureEntity Closure = new ValueNodeClosureEntity();
    public List<ValueNodeClosureRow> ClosureRows = [];

    private void Serialize(EfeuValue root)
    {
        
    }
}