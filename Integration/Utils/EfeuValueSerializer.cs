using System.Collections.Generic;
using Efeu.Integration.Entities;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Utils;

public class EfeuValueDeserializer
{
    public void Deserialize(byte[] bytes, ValueNodeRow[] nodes)
    {
        
    }
}

public class EfeuValueSerializer
{
    public List<ValueNodeRow> Nodes = [];
    
    private byte[] Serialize(EfeuValue root)
    {
        return [];
    }
}