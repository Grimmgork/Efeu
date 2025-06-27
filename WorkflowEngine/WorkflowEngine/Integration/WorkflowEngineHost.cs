using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;
using Workflows.Model;

namespace Workflows.Integration
{
    public class WorkflowEngineHost : IHostedService, IWorkflowEngineHost
    {
        private IServiceScopeFactory scopeFactory;

        public WorkflowEngineHost(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                // load all active workflow instances

                using (IServiceScope scope = scopeFactory.CreateScope())
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
    }
}
