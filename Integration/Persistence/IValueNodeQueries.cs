using System.Threading.Tasks;
using Efeu.Integration.Entities;

namespace Efeu.Integration.Persistence;

public interface IValueNodeQueries
{
    public Task InsertNodesAsync(ValueNodeEntity[] nodes);
    
    public Task InsertNodeReferencesAsync(ValueNodeReferenceEntity[] references);
    
    public Task<ValueNodeEntity[]> LoadAsync(string rootHash);

    public Task CleanupAsync();
}