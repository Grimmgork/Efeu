using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Services
{
    internal class TriggerCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public TriggerCleanupService(IServiceScopeFactory scopeFactory) 
        {
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // load all marked Triggers
            // decrement reference count of BehaviourScope
            // delete trigger
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                ITriggerQueries triggerQueries = scope.ServiceProvider.GetRequiredService<ITriggerQueries>();
                IBehaviourScopeQueries behaviourScopeQueries = scope.ServiceProvider.GetRequiredService<IBehaviourScopeQueries>();
                IEfeuUnitOfWork efeuUnitOfWork = scope.ServiceProvider.GetRequiredService<IEfeuUnitOfWork>();

                try
                {
                    TriggerEntity[] triggerEntityIds = await triggerQueries.GetDeletedAsync(25);

                    await efeuUnitOfWork.BeginAsync();
                    foreach (TriggerEntity triggerEntity in triggerEntityIds)
                    {
                        await behaviourScopeQueries.DecrementReferenceCountAsync(triggerEntity.ScopeId);
                        await triggerQueries.CleanupAsync(triggerEntity.Id);
                    }
                    await behaviourScopeQueries.DeleteUnreferencedAsync();
                    await efeuUnitOfWork.CompleteAsync();

                    if (!triggerEntityIds.Any())
                    {
                        await Task.Delay(TimeSpan.FromMinutes(10));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}
