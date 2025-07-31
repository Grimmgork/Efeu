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

namespace Efeu.Integration.Services
{
    public class WorkflowEngineHost : IHostedService, IWorkflowEngineHost
    {
        private IServiceScopeFactory scopeFactory;

        public IServiceScopeFactory ScopeFactory { get => scopeFactory; set => scopeFactory = value; }

        public WorkflowEngineHost(IServiceScopeFactory scopeFactory)
        {
            ScopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // load all active worklfow instances
            // 

            return Task.CompletedTask;
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

        public Task<SomeStruct> ExecuteWorkflowAsync(int id, SomeStruct input, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        Task<WorkflowInstance> IWorkflowEngineHost.StartWorkflowAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
