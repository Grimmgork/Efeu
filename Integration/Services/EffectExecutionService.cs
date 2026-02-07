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
            IEfeuEffectCommands behaviourEffectCommands = services.GetRequiredService<IEfeuEffectCommands>();
            IEfeuUnitOfWork unitOfWork = services.GetRequiredService<IEfeuUnitOfWork>();

            try
            {
                await unitOfWork.BeginAsync();
                await unitOfWork.BeginAsync();
                await unitOfWork.BeginAsync();
                EfeuMessage message = new EfeuMessage()
                {
                    Id = Guid.NewGuid(),
                    Tag = EfeuMessageTag.Data,
                    CorrelationId = Guid.NewGuid(),
                };
                await behaviourEffectCommands.SendMessage(message, DateTime.Now);
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
            EfeuEffectEntity? effect = await FindAndLockEffect(workerId, token);
            if (effect is null)
                return 0;

            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IBehaviourEffectQueries behaviourMessageRepository = services.GetRequiredService<IBehaviourEffectQueries>();
            IEfeuEffectCommands behaviourEffectCommands = services.GetRequiredService<IEfeuEffectCommands>();
            IEfeuUnitOfWork unitOfWork = services.GetRequiredService<IEfeuUnitOfWork>();
            IEfeuEffectProvider effectProvider = services.GetRequiredService<IEfeuEffectProvider>();

            DateTimeOffset executionTime = DateTime.Now;
            await unitOfWork.BeginAsync();

            try
            {
                if (effect.Tag == EfeuMessageTag.Effect)
                {
                    IEfeuEffect? effectInstance = effectProvider.TryGetEffect(effect.Name);
                    if (effectInstance is null)
                        throw new Exception($"Unknown effect '{effect.Name}'.");

                    EfeuEffectExecutionContext context = new EfeuEffectExecutionContext(effect.Id, effect.CorrelationId, executionTime, effect.Times, effect.Data);

                    await effectInstance.RunAsync(context, token);
                    await behaviourMessageRepository.CompleteEffectAndUnlockAsync(workerId, effect.Id, DateTime.Now, default);
                    await unitOfWork.CompleteAsync();
                }
                else
                {
                    EfeuMessage message = new EfeuMessage()
                    {
                        Id = effect.Id,
                        Tag = effect.Tag,
                        Name = effect.Name,
                        Data = effect.Data,
                        Timestamp = effect.CreationTime,
                        Matter = effect.Matter,
                        CorrelationId = effect.CorrelationId,
                    };

                    await behaviourEffectCommands.SendMessage(message, DateTime.Now);
                    await behaviourMessageRepository.DeleteCompletedSignalAsync(effect.Id);
                    await unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await behaviourMessageRepository.FaultEffectAndUnlockAsync(workerId, effect.Id, executionTime, ex.ToString());
                await unitOfWork.CompleteAsync();
            }
            
            return 1;
        }

        private async Task<EfeuEffectEntity?> FindAndLockEffect(Guid workerId, CancellationToken token)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IBehaviourEffectQueries behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectQueries>();

            Guid[] candidateIds = await behaviourEffectRepository.GetRunningEffectsNotLockedAsync(DateTime.Now);
            EfeuEffectEntity? effect = null;
            foreach (Guid candidateId in candidateIds)
            {
                DateTimeOffset timestamp = DateTime.Now;
                if (await behaviourEffectRepository.TryLockEffectAsync(candidateId, workerId, timestamp, TimeSpan.FromSeconds(30)))
                {
                    effect = await behaviourEffectRepository.GetByIdAsync(candidateId);
                    if (effect is not null)
                        break;
                }
            }

            return effect;
        }
    }
}
