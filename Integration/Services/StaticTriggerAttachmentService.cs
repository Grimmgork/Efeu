using Efeu.Integration.Persistence;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Services
{
    internal class StaticTriggerAttachmentService : IHostedService
    {
        private readonly IBehaviourTriggerRepository triggerRepository;
        public StaticTriggerAttachmentService(IBehaviourTriggerRepository triggerRepository) 
        { 
        this.triggerRepository = triggerRepository;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // read all triggers

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
