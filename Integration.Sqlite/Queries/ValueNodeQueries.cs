using System.Collections.Generic;
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
        DataParameter[] parameters = hashes.Select((h, i) => new DataParameter($"@{i}", h)).ToArray();
        IEnumerable<ValueNodeEntity> nodes = await connection.QueryAsync<ValueNodeEntity>(
            $"""
             WITH RECURSIVE reachable(hash) AS (
                 VALUES {string.Join(", ", parameters.Select(i => $"({i.Name})"))}

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
            parameters
        );
        
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

    public async Task CleanupAsync()
    {
        throw new System.NotImplementedException();
    }
}