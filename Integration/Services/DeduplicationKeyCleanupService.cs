using Efeu.Integration.Commands;
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
    internal class DeduplicationKeyCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public DeduplicationKeyCleanupService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                IDeduplicationKeyCommands deduplicationKeyCommands = scope.ServiceProvider.GetRequiredService<IDeduplicationKeyCommands>();

                try
                {
                    DateTimeOffset timestamp = DateTimeOffset.UtcNow;
                    await deduplicationKeyCommands.CleanupAsync(timestamp);
                    await Task.Delay(TimeSpan.FromMinutes(5));
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
