using System.Threading.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Utils.Serialization;

namespace Efeu.Integration.Persistence;

public interface IValueNodeQueries
{
    public Task WriteAsync(EfeuValueSerializationResult serialization);
    
    public Task<EfeuValueSerializationResult> ReadAsync(string[] hashes);

    public Task CleanupAsync();
}