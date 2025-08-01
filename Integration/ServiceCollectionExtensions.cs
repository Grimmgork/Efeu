using Efeu.Integration.Commands;
using Efeu.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEfeu(this IServiceCollection services)
        {
            services.AddScoped<IWorkflowDefinitionCommands, WorkflowDefinitionCommands>();
            services.AddScoped<IWorkflowInstanceCommands, WorkflowInstanceCommands>();
            services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        }
    }
}
