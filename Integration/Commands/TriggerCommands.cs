using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands;

internal class TriggerCommands : ITriggerCommands
{
    private readonly IEfeuUnitOfWork unitOfWork;
    private readonly ITriggerQueries triggerQueries;

    public TriggerCommands(IEfeuUnitOfWork unitOfWork, ITriggerQueries triggerQueries, IEfeuTriggerProvider triggerProvider)
    {
        this.unitOfWork = unitOfWork;
        this.triggerQueries = triggerQueries;
    }

    public async Task DeleteStaticAsync(Guid definitionVersionId)
    {
        await unitOfWork.BeginAsync();
        await unitOfWork.LockAsync("Trigger");
        await triggerQueries.DetatchStaticAsync(definitionVersionId);
        await unitOfWork.CompleteAsync();
    }

    public Task DeleteAsync(Guid[] ids)
    {
        return triggerQueries.DetatchAsync(ids);
    }
}
