using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowEngineIntegration;
using Workflows.Data;
using Workflows.Method;

namespace Workflows
{
    public class WorkflowMethodPrintBusiness : WorkflowMethodBase
    {
        private readonly IServiceScopeFactory scopeFactory;

        public WorkflowMethodPrintBusiness(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public override async Task<WorkflowMethodState> RunAsync(WorkflowMethodContext context, CancellationToken token)
        {
            using (IServiceScope scope = scopeFactory.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IBusinessLogicService>().Hello();
            }
            return WorkflowMethodState.Done;
        }
    }
}
