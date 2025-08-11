using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;

namespace Efeu.Runtime
{
    public class DefaultWorkflowActionInstanceFactory : IDefaultWorkflowActionInstanceFactory
    {
        private Dictionary<string, Func<IWorkflowMethod>> methods = new Dictionary<string, Func<IWorkflowMethod>>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, Func<IWorkflowFunctionInstance>> functions = new Dictionary<string, Func<IWorkflowFunctionInstance>>(StringComparer.InvariantCultureIgnoreCase);

        public DefaultWorkflowActionInstanceFactory()
        {
            
        }

        public IWorkflowMethod GetMethodInstance(string name)
        {
            return methods[name].Invoke();
        }

        public IWorkflowFunctionInstance GetFunctionInstance(string name)
        {
            return functions[name].Invoke();
        }

        public void Register(string name, Func<IWorkflowFunctionInstance> build)
        {
            functions.Add(name, build);
        }

        public void Register(string name, Func<IWorkflowMethod> build)
        {
            methods.Add(name, build);
        }
    }
}
