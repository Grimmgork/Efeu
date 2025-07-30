using Efeu;
using Efeu.Runtime;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;

namespace Efeu.Runtime
{
    public interface IDefaultWorkflowActionInstanceFactory : IWorkflowActionInstanceFactory
    {
        public void Register(string name, Func<IWorkflowFunctionInstance> build);

        public void Register(string name, Func<IWorkflowMethodInstance> build);
    }
}
