using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
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

        public async Task Test()
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IDeduplicationKeyCommands deduplicationKeyCommands = services.GetRequiredService<IDeduplicationKeyCommands>();
            IEffectCommands behaviourEffectCommands = services.GetRequiredService<IEffectCommands>();
            IEfeuUnitOfWork unitOfWork = services.GetRequiredService<IEfeuUnitOfWork>();

            try
            {
                await unitOfWork.BeginAsync();
                await unitOfWork.BeginAsync();
                await unitOfWork.BeginAsync();
                await unitOfWork.CompleteAsync();
                await unitOfWork.CompleteAsync();
                await unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // return Test();

            CancellationToken token = cancellationTokenSource.Token;

            List<Task> workers = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                workers.Add(Task.Run(async () =>
                {
                    Guid workerId = Guid.NewGuid();
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            int execution = await ExecuteEffect(workerId, token);
                            if (execution == 0)
                            {
                                await Task.Delay(1000, cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
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

            IEffectQueries effectQueries = scope.ServiceProvider.GetRequiredService<IEffectQueries>();
            IEffectCommands effectCommands = scope.ServiceProvider.GetRequiredService<IEffectCommands>();
            IEfeuUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IEfeuUnitOfWork>();
            IEfeuEffectProvider effectProvider = scope.ServiceProvider.GetRequiredService<IEfeuEffectProvider>();

            EffectEntity? effect = await FindAndLockEffect(effectQueries, workerId, token);
            if (effect is null)
                return 0;

            DateTimeOffset executionTime = DateTime.Now;
            try
            {
                if (effect.Tag == EfeuMessageTag.Effect)
                {
                    IEfeuEffect? effectInstance = effectProvider.TryGetEffect(effect.Type);
                    if (effectInstance is null)
                        throw new Exception($"Unknown effect '{effect.Type}'.");

                    EfeuEffectExecutionContext context = new EfeuEffectExecutionContext(effect.Id, effect.CorrelationId, executionTime, effect.Times, effect.Input);

                    await effectInstance.RunAsync(context, token);
                    await effectQueries.CompleteEffectAndUnlockAsync(workerId, effect.Id, DateTime.Now, default);
                }
                else
                {
                    await unitOfWork.BeginAsync();
                    EfeuMessage message = new EfeuMessage()
                    {
                        Id = effect.Id,
                        Tag = effect.Tag,
                        Type = effect.Type,
                        Payload = effect.Data,
                        Timestamp = effect.CreationTime,
                        Matter = effect.Matter,
                        CorrelationId = effect.CorrelationId,
                    };

                    await effectCommands.SendMessageDeduplicatedAsync(message);
                    await effectQueries.DeleteCompletedEffectAsync(workerId, effect.Id);
                    await unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                await unitOfWork.ResetAsync();

                Console.WriteLine(ex.ToString());
                await effectQueries.FaultEffectAndUnlockAsync(workerId, effect.Id, executionTime, ex.ToString());
            }
            
            return 1;
        }

        private async Task<EffectEntity?> FindAndLockEffect(IEffectQueries effectQueries, Guid workerId, CancellationToken token)
        {
            Guid[] candidateIds = await effectQueries.GetRunningEffectsNotLockedAsync();
            EffectEntity? effect = null;
            foreach (Guid candidateId in candidateIds)
            {
                DateTimeOffset timestamp = DateTime.Now;
                if (await effectQueries.TryLockEffectAsync(candidateId, workerId, TimeSpan.FromSeconds(30)))
                {
                    effect = await effectQueries.GetByIdAsync(candidateId);
                    if (effect is not null)
                        break;
                }
            }

            return effect;
        }
    }
}
