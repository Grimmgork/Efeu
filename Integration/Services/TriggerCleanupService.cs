using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
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
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                ITriggerQueries triggerQueries = scope.ServiceProvider.GetRequiredService<ITriggerQueries>();
                IBehaviourScopeQueries behaviourScopeQueries = scope.ServiceProvider.GetRequiredService<IBehaviourScopeQueries>();
                IEfeuUnitOfWork efeuUnitOfWork = scope.ServiceProvider.GetRequiredService<IEfeuUnitOfWork>();

                try
                {
                    TriggerEntity[] triggerEntities = await triggerQueries.GetDetatchedAsync(25);
                    Guid[] triggerIds = triggerEntities.Select(i => i.Id).ToArray();
                    Dictionary<Guid, uint> decrements = new Dictionary<Guid, uint>();
                    foreach (TriggerEntity triggerEntity in triggerEntities)
                    {
                        if (decrements.ContainsKey(triggerEntity.ScopeId))
                        {
                            decrements[triggerEntity.ScopeId]++;
                        }
                        else
                        {
                            decrements[triggerEntity.ScopeId] = 1;
                        }
                    }

                    await efeuUnitOfWork.BeginAsync();
                    await behaviourScopeQueries.DecrementReferenceCountAsync(decrements);
                    await triggerQueries.DeleteAsync(triggerIds);
                    await efeuUnitOfWork.CompleteAsync();

                    await behaviourScopeQueries.CleanupAsync();

                    if (triggerEntities.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
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
