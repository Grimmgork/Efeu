using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime;
using Efeu.Runtime.Model;
using Efeu.Runtime.Data;
using System.Threading;

namespace Efeu.Integration
{
    public class WorkflowEngineHost : IHostedService, IWorkflowEngineHost
    {
        private IServiceScopeFactory scopeFactory;

        public IServiceScopeFactory ScopeFactory { get => scopeFactory; set => scopeFactory = value; }

        public WorkflowEngineHost(IServiceScopeFactory scopeFactory)
        {
            this.ScopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                // load all active workflow instances

                ScopeFactory.CreateScope();

                using (IServiceScope scope = ScopeFactory.CreateScope())
                {
                    // run workflow instance
                    IWorkflowActionInstanceFactory instanceFactory = scope.ServiceProvider.GetRequiredService<IWorkflowActionInstanceFactory>();
                    IWorkflowSignalHandler signalHandler = scope.ServiceProvider.GetRequiredService<IWorkflowSignalHandler>();
                    // WorkflowInstance instance = new WorkflowInstance();


                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task PublishWorkflowDefinitionVersion(WorkflowDefinition definition)
        {
            return Task.CompletedTask;
        }

        public Task<WorkflowInstance> StartWorkflowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<SomeDataStruct> ExecuteWorkflowAsync(int id, SomeDataStruct input, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        Task<WorkflowInstance> IWorkflowEngineHost.StartWorkflowAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
