using Efeu.Integration.Foreign;
using Efeu.Runtime;
using Microsoft.AspNetCore.Http.Connections;
using System.Threading.Tasks;

namespace Efeu.Application.Services
{
    public class WorkflowRuntimeEnvironmentFactory(
        IWorkflowMethodProvider methodProvider, 
        IWorkflowFunctionProvider functionProvider, 
        IWorkflowTriggerProvider triggerProvider
    ) : IWorkflowRuntimeEnvironmentFactory
    {

        public Task<WorkflowRuntimeEnvironment> CreateAsync()
        {
            return Task.FromResult(new WorkflowRuntimeEnvironment(methodProvider, functionProvider, triggerProvider, null));
        }
    }
}
