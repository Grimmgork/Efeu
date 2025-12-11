using Antlr4.Build.Tasks;
using Antlr4.Runtime;
using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
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
    internal class EffectExecutionService : IHostedService
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

            List<Task> workers = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                workers.Add(Task.Run(async () =>
                {
                    Guid workerId = Guid.NewGuid();
                    while (!token.IsCancellationRequested)
                    {
                        Console.WriteLine($"loop {workerId}");
                        try
                        {
                            int execution = await ExecuteEffect(workerId, token);
                            if (execution == 0)
                            {
                                await Task.Delay(1000, cancellationToken);
                            }
                        }
                        catch (Exception)
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                    }
                }));
            }

            work = Task.WhenAll(workers);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource.Cancel();
            return work;
        }

        private async Task<int> ExecuteEffect(Guid workerId, CancellationToken token)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IBehaviourEffectRepository behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectRepository>();
            IUnitOfWork unitOfWork = services.GetRequiredService<IUnitOfWork>();
            EfeuEnvironment environment = services.GetRequiredService<EfeuEnvironment>();

            int[] candidateIds = await behaviourEffectRepository.GetRunningEffectNotLockedAsync(DateTime.Now);
            BehaviourEffectEntity? effect = null;
            foreach (int candidateId in candidateIds)
            {
                DateTime timestamp = DateTime.Now;
                if (await behaviourEffectRepository.TryLockAsync(candidateId, workerId, timestamp, TimeSpan.FromSeconds(30)))
                {
                    effect = await behaviourEffectRepository.GetByIdAsync(candidateId);
                    if (effect != null)
                        continue;
                }
            }

            if (effect == null)
                return 0;

            try
            {
                IEffect? effectInstance = environment.EffectProvider.TryGetEffect(effect.Name);
                if (effectInstance is null)
                    throw new Exception($"Unknown effect '{effect.Name}'.");

                // run effect
                EffectExecutionContext context = new EffectExecutionContext(effect.Id, effect.CorrelationId, effect.Times, effect.Data);
                await effectInstance.RunAsync(context, default);

                await unitOfWork.BeginAsync();
                if (effect.TriggerId != Guid.Empty)
                {
                    // send response message
                    await behaviourEffectRepository.CreateAsync(new BehaviourEffectEntity()
                    {
                        CreationTime = DateTime.Now,
                        Name = effect.Name,
                        TriggerId = effect.TriggerId,
                        Input = context.Output,
                        CorrelationId = effect.CorrelationId,
                        State = BehaviourEffectState.Running
                    });
                }
                await behaviourEffectRepository.DeleteAsync(effect.Id);
                await unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.BeginAsync();
                await behaviourEffectRepository.MarkErrorAsync(effect.Id, effect.Times + 1);
                await behaviourEffectRepository.UnlockAsync(workerId);
                await unitOfWork.CommitAsync();
            }

            return 1;
        }
    }
}
