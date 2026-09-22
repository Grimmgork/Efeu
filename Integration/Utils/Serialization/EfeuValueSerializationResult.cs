using System.Collections.Generic;
using Efeu.Integration.Entities;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueSerializationResult
{
    public string Hash = "";

    public string[] Hashes = [];
    
    public Dictionary<string, ValueNodeEntity> Nodes = new ();

    public List<ValueNodeReferenceEntity> References = [];
}