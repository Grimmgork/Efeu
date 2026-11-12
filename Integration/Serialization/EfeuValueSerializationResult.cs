using System.Collections.Generic;
using Efeu.Integration.Entities;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueSerializationResult
{
    public EfeuValueHash[] Hashes = [];
    
    public Dictionary<EfeuValueHash, ValueNodeEntity> Nodes = new ();

    public List<ValueNodeReferenceEntity> References = [];
}