using System;
using System.Collections.Generic;
using System.Linq;
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
        
        EfeuValueSerializerOptions serializerOptions = new()
        {
            Writer = new EfeuValueBinaryWriter(),
            Hasher = new Sha256EfeuValueHasher()
        };
        
        EfeuValueSerializationResult result = EfeuValueSerializer.Serialize(value, serializerOptions);

        EfeuValueDeserializerOptions deserializerOptions = new EfeuValueDeserializerOptions()
        {
            Reader = new EfeuValueBinaryReader()
        };
        
        value = EfeuValueDeserializer.Deserialize(result, deserializerOptions);
        
        await valueNodeQueries.WriteAsync(result);
        await unitOfWork.CompleteAsync();
        return result.Hash;
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