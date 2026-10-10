using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Async;

namespace Efeu.Integration.Sqlite.Queries;

internal class TriggerQueries : ITriggerQueries
{
    private readonly DataConnection connection;

    public TriggerQueries(DataConnection connection)
    {
        this.connection = connection;
    }

    public Task<int> CreateAsync(TriggerEntity trigger)
    {
        return connection.InsertWithInt32IdentityAsync(trigger);
    }

    public Task CreateBulkAsync(TriggerEntity[] triggers)
    {
        return connection.BulkCopyAsync(triggers);
    }

    public Task DetatchAsync(Guid[] ids)
    {
        if (ids.Length == 0)
            return Task.CompletedTask;

        return connection.GetTable<TriggerEntity>()
            .Where(i => ids.AsEnumerable().Contains(i.Id))
            .DeleteAsync();
    }

    public Task DetatchStaticAsync(Guid definitionVersionId)
    {
        return connection.GetTable<TriggerEntity>()
            .Where(i => i.BehaviourVersionId == definitionVersionId 
                          && i.CorrelationId == Guid.Empty)
            .DeleteAsync();
    }

    public Task DetatchByMatterBulkAsync(Guid[] matters)
    {
        if (matters.Length == 0)
            return Task.CompletedTask;

        return connection.GetTable<TriggerEntity>()
            .Where(i => matters.AsEnumerable().Contains(i.Matter))
            .DeleteAsync();
    }

    public Task DetatchByGroupBulkAsync(Guid[] groups)
    {
        if (groups.Length == 0)
            return Task.CompletedTask;

        return connection.GetTable<TriggerEntity>()
            .Where(i => groups.AsEnumerable().Contains(i.Group))
            .DeleteAsync();
    }

    public Task<TriggerEntity[]> GetAllAsync()
    {
        return connection.GetTable<TriggerEntity>()
           .ToArrayAsync();
    }

    public Task<TriggerEntity[]> GetStaticAsync(Guid definitionVersionId)
    {
        return connection.GetTable<TriggerEntity>()
            .Where(i => i.BehaviourVersionId == definitionVersionId
                     && i.CorrelationId == Guid.Empty)
            .ToArrayAsync();
    }

    public Task<TriggerEntity?> GetByIdAsync(Guid id)
    {
        return connection.GetTable<TriggerEntity>()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<TriggerEntity[]> GetByIdsAsync(params Guid[] ids)
    {
        if (ids.Length == 0)
        {
            return [];
        }
        else if (ids.Length == 1)
        {
            TriggerEntity? entity = await GetByIdAsync(ids.First());
            if (entity == null)
            {
                return [];
            }
            else
            {
                return [entity];
            }
        }
        else
        {
            return await connection.GetTable<TriggerEntity>()
                .Where(i => ids.AsEnumerable().Contains(i.Id))
                .ToArrayAsync();
        }
    }
}
