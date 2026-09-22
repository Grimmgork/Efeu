using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime;

namespace Efeu.Integration.Commands;

internal class BehaviourScopeCommands : IBehaviourScopeCommands
{
    private readonly IBehaviourScopeQueries behaviourScopeQueries;
    private readonly IEfeuUnitOfWork unitOfWork;

    public BehaviourScopeCommands(IBehaviourScopeQueries behaviourScopeQueries, IEfeuUnitOfWork unitOfWork)
    {
        this.behaviourScopeQueries = behaviourScopeQueries;
        this.unitOfWork = unitOfWork;
    }

    public async Task GetByIdAsync(Guid id)
    {
        
    }

    public async Task CreateBulkAsync(BehaviourScopeEntity[] entities)
    {
        await unitOfWork.BeginAsync();
        await behaviourScopeQueries.CreateBulkAsync(entities);
        await unitOfWork.CompleteAsync();
    }
}
