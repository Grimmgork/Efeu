using Efeu.Runtime.Function;
using Efeu.Runtime.Method;

namespace Efeu
{
    public interface IWorkflowActionInstanceFactory
    {
        public IWorkflowFunctionInstance GetFunctionInstance(string name);

        public IWorkflowMethodInstance GetMethodInstance(string name);
    }
}