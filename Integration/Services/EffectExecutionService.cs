using Antlr4.Runtime;
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
    public interface IEffectExecutionService : IHostedService
    {
        public void Nudge(int id);
    }

    internal class EffectExecutionService : IEffectExecutionService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task work = Task.CompletedTask;

        public EffectExecutionService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CancellationToken token = cancellationTokenSource.Token;

            work = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    using (IServiceScope scope = scopeFactory.CreateScope())
                    {
                        try
                        {
                            await Execute(scope.ServiceProvider, token);
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }

                    await Task.Delay(1000);
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource.Cancel();
            return work;
        }

        public void Nudge(int id)
        {
            throw new NotImplementedException();
        }

        private async Task Execute(IServiceProvider services, CancellationToken token)
        {
            IBehaviourEffectCommands behaviourEffectCommands = services.GetRequiredService<IBehaviourEffectCommands>();
            IBehaviourEffectRepository behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectRepository>();

            BehaviourEffectEntity[] effects = await behaviourEffectRepository.GetRunningAsync(20);
            foreach (BehaviourEffectEntity effect in effects)
            {
                await behaviourEffectCommands.RunEffect(effect.Id);
            }
        }
    }
}
