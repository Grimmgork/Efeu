using Microsoft.Extensions.Hosting;
using Workflows;

namespace WorkflowEngineIntegration
{
    public class ModuleActivator
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IBusinessLogicService, BusinessLogicService>();
        }

        public void RegisterWorkflowFunctions(IDefaultWorkflowActionInstanceFactory factory, IServiceScopeFactory scopeFactory)
        {
            factory.Register("PrintBusiness", () => new WorkflowMethodPrintBusiness(scopeFactory));
        }
    }
}
