using Efeu.Integration.Interfaces;
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
            services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        }
    }
}
