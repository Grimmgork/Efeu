using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Integration.Utils;
using Efeu.Integration.Utils.Serialization;
using Efeu.Runtime.Value;

namespace Efeu.Integration.Commands;

public class ValueNodeCommands
{
    private IEfeuUnitOfWork unitOfWork;
    private IValueNodeQueries valueNodeQueries;
    
    public ValueNodeCommands(IEfeuUnitOfWork unitOfWork, IValueNodeQueries valueNodeQueries)
    {
        this.unitOfWork = unitOfWork;
        this.valueNodeQueries = valueNodeQueries;
    }

    public async Task<string> WriteAsync(EfeuValue value)
    {
        await unitOfWork.BeginAsync();
        // traverse value
        // generate payloads
        // generate hashes
        // generate entities
        // insert entities
        
        Dictionary<string, byte[]> payloads = new Dictionary<string, byte[]>();
        
        EfeuValueSerializerOptions serializerOptions = new()
        {
            Visit = (hash, payload) => payloads.Add(hash, payload),
            Writer = new EfeuValueBinaryWriter(),
            Hasher = new Sha256EfeuValueHasher()
        };
        
        string rootHash = EfeuValueSerializer.Serialize(value, serializerOptions);

        EfeuValueDeserializerOptions deserializerOptions = new EfeuValueDeserializerOptions()
        {
            Resolve = (hash) => payloads[hash],
            Reader = new EfeuValueBinaryReader()
        };
        
        EfeuValue result = EfeuValueDeserializer.Deserialize(rootHash, deserializerOptions);
        
        await valueNodeQueries.InsertNodesAsync([]);
        await valueNodeQueries.InsertNodeReferencesAsync([]);
        await unitOfWork.CompleteAsync();
        return rootHash;
    }
    
    public Task<EfeuValue> ReadAsync(string rootHash)
    {
        // run query
        // traverse all nodes
        // deserialize payload
        // reconstruct value

        throw new NotImplementedException();
    }
}