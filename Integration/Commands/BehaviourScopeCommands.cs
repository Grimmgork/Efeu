using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime;
using Efeu.Runtime.Value.Reference;

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

        List<ValueNodeClosureEntity> edgeEntities = [];
        // hash values
        Sha256EfeuValueReferenceHasher valueReferenceHasher = new Sha256EfeuValueReferenceHasher((value, reference) =>
        {
            edgeEntities.Add(new );
        });
        
        // BehaviourScopeEdge
        // Name
        // ScopeId
        // Value
        
        await behaviourScopeQueries.CreateBulkAsync(entities);
        await unitOfWork.CompleteAsync();
    }

    private static EfeuRuntimeScope ConstructScope()
    {
        
    }
}
