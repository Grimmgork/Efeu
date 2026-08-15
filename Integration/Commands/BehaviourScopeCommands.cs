using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands;

internal class BehaviourScopeCommands : IBehaviourScopeCommands
{
    private readonly IBehaviourScopeQueries behaviourScopeQueries;

    public BehaviourScopeCommands(IBehaviourScopeQueries behaviourScopeQueries)
    {
        this.behaviourScopeQueries = behaviourScopeQueries;
    }

    public async Task CreateBulkAsync(BehaviourScopeEntity[] entities)
    {
        await behaviourScopeQueries.CreateBulkAsync(entities);
    }
}
