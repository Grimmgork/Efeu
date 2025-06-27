using Workflows;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Workflows.Function;
using Workflows.Method;

namespace WorkflowEngineIntegration
{
    public interface IDefaultWorkflowActionInstanceFactory : IWorkflowActionInstanceFactory
    {
        public void Register(string name, Func<IWorkflowFunctionInstance> build);

        public void Register(string name, Func<IWorkflowMethodInstance> build);
    }
}
