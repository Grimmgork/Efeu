using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;
using WorkflowEngineIntegration;

namespace Efeu
{
    public class DefaultWorkflowFunctionInstanceFactory : IDefaultWorkflowActionInstanceFactory
    {
        private Dictionary<string, Func<IWorkflowMethodInstance>> methods = new Dictionary<string, Func<IWorkflowMethodInstance>>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, Func<IWorkflowFunctionInstance>> functions = new Dictionary<string, Func<IWorkflowFunctionInstance>>(StringComparer.InvariantCultureIgnoreCase);

        public DefaultWorkflowFunctionInstanceFactory()
        {
            
        }

        public IWorkflowMethodInstance GetMethodInstance(string name)
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

        public void Register(string name, Func<IWorkflowMethodInstance> build)
        {
            methods.Add(name, build);
        }
    }
}
