using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;

namespace Efeu.Runtime
{
    public class SimpleWorkflowFunctionProvider : IWorkflowFunctionProvider
    {
        
        private Dictionary<string, Func<IWorkflowFunction>> functions = new Dictionary<string, Func<IWorkflowFunction>>(StringComparer.InvariantCultureIgnoreCase);

        public SimpleWorkflowFunctionProvider()
        {
            
        }

        public void Register(string name, Func<IWorkflowFunction> build)
        {
            functions.Add(name, build);
        }

        public IWorkflowFunction GetFunction(string name)
        {
            return functions[name].Invoke();
        }


    }
}
