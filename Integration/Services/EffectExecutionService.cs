using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Router.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

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

        private async Task Test()
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IEfeuUnitOfWork unitOfWork = services.GetRequiredService<IEfeuUnitOfWork>();
            IEfeuEngine efeu = services.GetRequiredService<IEfeuEngine>();

            await unitOfWork.DoAsync(async () =>
            {
                await efeu.ProcessSignalAsync(new EfeuMessage(), Guid.NewGuid(), DateTime.Now);
            });
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
            BehaviourEffectEntity? effect = await FindAndLockEffect(workerId, token);
            if (effect is null)
                return 0;

            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IBehaviourEffectRepository behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectRepository>();
            IBehaviourEffectCommands behaviourEffectCommands = services.GetRequiredService<IBehaviourEffectCommands>();
            IEfeuUnitOfWork unitOfWork = services.GetRequiredService<IEfeuUnitOfWork>();
            IEfeuEffectProvider effectProvider = services.GetRequiredService<IEfeuEffectProvider>();

            DateTimeOffset executionTime = DateTime.Now;
            await unitOfWork.BeginAsync();
            try
            {
                if (effect.Tag == BehaviourEffectTag.Outgoing)
                {
                    IEffect? effectInstance = effectProvider.TryGetEffect(effect.Name);
                    if (effectInstance is null)
                        throw new Exception($"Unknown effect '{effect.Name}'.");

                    EffectExecutionContext context = new EffectExecutionContext(effect.Id, effect.CorrelationId, executionTime, effect.Times, effect.Data);

                    await effectInstance.RunAsync(context, token);
                    await behaviourEffectRepository.CompleteEffectAndUnlockAsync(workerId, effect.Id, DateTime.Now, context.Output);
                }
                else
                {
                    EfeuMessage message = new EfeuMessage()
                    {
                        Tag = EfeuMessageTag.Incoming,
                        Name = effect.Name,
                        CorrelationId = effect.CorrelationId,
                        Data = effect.Data,
                        TriggerId = effect.TriggerId
                    };

                    await behaviourEffectCommands.ProcessSignal(message, Guid.NewGuid(), DateTime.Now);
                    await behaviourEffectRepository.DeleteCompletedSignalAsync(effect.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await behaviourEffectRepository.FaultEffectAndUnlockAsync(workerId, effect.Id, executionTime, ex.ToString());
            }

            await unitOfWork.CompleteAsync();
            return 1;
        }

        private async Task<BehaviourEffectEntity?> FindAndLockEffect(Guid workerId, CancellationToken token)
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            IServiceProvider services = scope.ServiceProvider;
            IBehaviourEffectRepository behaviourEffectRepository = services.GetRequiredService<IBehaviourEffectRepository>();

            int[] candidateIds = await behaviourEffectRepository.GetRunningEffectsNotLockedAsync(DateTime.Now);
            BehaviourEffectEntity? effect = null;
            foreach (int candidateId in candidateIds)
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
