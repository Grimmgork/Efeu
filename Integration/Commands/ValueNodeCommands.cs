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

    public async Task<string[]> WriteAsync(EfeuValue[] values)
    {
        await unitOfWork.BeginAsync();
        EfeuValueSerializerOptions serializerOptions = new()
        {
            Writer = new EfeuValueBinaryWriter(),
            Hasher = new Sha256EfeuValueHasher()
        };
        
        EfeuValueSerializationResult result = EfeuValueSerializer.Serialize(values, serializerOptions);
        
        await valueNodeQueries.WriteAsync(result);
        await unitOfWork.CompleteAsync();
        return result.Hashes;
    }
    
    public async Task<EfeuValue[]> ReadAsync(string[] hashes)
    {
        EfeuValueSerializationResult result = await valueNodeQueries.ReadAsync(hashes);
        EfeuValueDeserializerOptions deserializerOptions = new EfeuValueDeserializerOptions()
        {
            Reader = new EfeuValueBinaryReader()
        };
        
        EfeuValue[] values = EfeuValueDeserializer.Deserialize(result, deserializerOptions);
        return values;
    }
    
    public Task CleanupAsync()
    {
        return valueNodeQueries.CleanupAsync();
    }
}