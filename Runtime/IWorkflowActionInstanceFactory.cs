using Efeu.Runtime.Function;
using Efeu.Runtime.Method;

namespace Efeu.Runtime
{
    public interface IWorkflowActionInstanceFactory
    {
        public IWorkflowFunctionInstance GetFunctionInstance(string name);

        public IWorkflowMethod GetMethodInstance(string name);
    }
}