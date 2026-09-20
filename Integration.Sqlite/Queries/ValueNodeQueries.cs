using System.Threading.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Integration.Utils.Serialization;
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

    public async Task WriteAsync(EfeuValueSerializationResult serialization)
    {
        await connection.GetTable<ValueNodeEntity>()
            .BulkCopyAsync(new BulkCopyOptions
            {
                ConflictAction = ConflictAction.Ignore,
                MaxBatchSize = 100
            }, serialization.Nodes.Values);
        
        await connection.GetTable<ValueNodeReferenceEntity>()
            .BulkCopyAsync(new BulkCopyOptions
            {
                ConflictAction = ConflictAction.Ignore,
                MaxBatchSize = 100
            }, serialization.References);
    }

    public Task<EfeuValueSerializationResult> ReadAsync(string hash)
    {
        throw new System.NotImplementedException();
    }

    public Task CleanupAsync()
    {
        throw new System.NotImplementedException();
    }
}