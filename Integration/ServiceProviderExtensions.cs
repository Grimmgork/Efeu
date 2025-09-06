using Efeu.Integration.Logic;
using Efeu.Integration.Model;
using Efeu.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public static class ServiceProviderExtensions
    {
        public static async Task<IWorkflowMethodProvider> BuildMethodProviderAsync(this IServiceProvider services)
        {
            IWorkflowMethodProvider? provider = services.GetService<IWorkflowMethodProvider>();
            if (provider != null)
                return provider;

            List<WorkflowMethodDescription> result = [];
            foreach (Task<IEnumerable<WorkflowMethodDescription>> descriptions in services.GetServices<Task<IEnumerable<WorkflowMethodDescription>>>())
            {
                result.AddRange(await descriptions);
            }

            foreach (WorkflowMethodDescription factory in services.GetServices<WorkflowMethodDescription>())
            {
                result.Add(factory);
            }

            SimpleWorkflowMethodProvider simpleProvider = new SimpleWorkflowMethodProvider();
            foreach (WorkflowMethodDescription description in result)
            {
                simpleProvider.Register(description.Name, description.Factory!);
            }

            return simpleProvider;
        }

        public static Task<IWorkflowFunctionProvider> BuildFunctionProvider(this IServiceProvider services)
        {
            throw new Exception();
        }

    }
}
