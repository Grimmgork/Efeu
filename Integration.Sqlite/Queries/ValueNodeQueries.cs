using System.Threading.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;

namespace Efeu.Integration.Sqlite.Queries;

public class ValueNodeQueries : IValueNodeQueries
{
    private readonly DataConnection connection;
    
    public ValueNodeQueries(DataConnection connection)
    {
        this.connection = connection;
    }
    
    public Task InsertNodesAsync(ValueNodeEntity[] nodes)
    {
        return connection.GetTable<ValueNodeEntity>()
            .BulkCopyAsync(nodes);
    }
    
    public Task InsertNodeReferencesAsync(ValueNodeReferenceEntity[] references)
    {
        return connection.GetTable<ValueNodeReferenceEntity>()
            .BulkCopyAsync(new BulkCopyOptions
            {
                ConflictAction = ConflictAction.Ignore,
                MaxBatchSize = 5000
            }, references);
    }

    public Task<ValueNodeEntity[]> LoadAsync(string rootHash)
    {
        throw new System.NotImplementedException();
    }

    public Task CleanupAsync()
    {
        throw new System.NotImplementedException();
    }
}