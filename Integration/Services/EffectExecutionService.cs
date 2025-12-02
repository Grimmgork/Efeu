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
                    try
                    {
                        await Execute(token);
                    }
                    catch (Exception ex)
                    {

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

        private async Task Execute(CancellationToken token)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            
            IBehaviourEffectCommands behaviourEffectCommands = services.GetRequiredService<IBehaviourEffectCommands>();
            IBehaviourEffectRepository behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectRepository>();
            IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();

            await unitOfWork.BeginAsync();
            BehaviourEffectEntity[] effects = await behaviourEffectRepository.GetRunningAsync(20);
            foreach (BehaviourEffectEntity effect in effects)
            {
                await behaviourEffectCommands.RunEffect(effect);
                await unitOfWork.CommitAsync();

                if (token.IsCancellationRequested)
                    break;
            }
        }
    }
}
