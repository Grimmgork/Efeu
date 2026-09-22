using System.Linq;
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

    public async Task<EfeuValueSerializationResult> ReadAsync(string[] hashes)
    {
        var query = await connection.QueryAsync<ValueNodeEntity>(
            """
            WITH RECURSIVE reachable(hash) AS (
                SELECT @rootHash

                UNION

                SELECT r.TargetHash
                FROM ValueNodeReference r
                JOIN reachable x
                  ON r.SourceHash = x.Hash
            )
            SELECT n.Hash, n.Payload
            FROM reachable x
            JOIN ValueNode n
              ON n.Hash = x.Hash
            """,
            new DataParameter("rootHash", hash)
        );
        
        ValueNodeEntity[] nodes = query.ToArray();

        EfeuValueSerializationResult result = new EfeuValueSerializationResult()
        {
            Hashes = hashes,
        };

        foreach (ValueNodeEntity node in nodes)
        {
            result.Nodes.Add(node.Hash, node);
        }

        return result;
    }

    public Task CleanupAsync()
    {
        throw new System.NotImplementedException();
    }
}