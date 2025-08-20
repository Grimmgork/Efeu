using Efeu.Runtime;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Efeu.Integration.Model
{
    public class SimpleWorkflowMethodProvider : IWorkflowMethodProvider
    {
        private Dictionary<string, Func<IWorkflowMethod>> methods = new Dictionary<string, Func<IWorkflowMethod>>(StringComparer.InvariantCultureIgnoreCase);

        public SimpleWorkflowMethodProvider()
        {

        }

        public IWorkflowMethod GetMethod(string name)
        {
            return methods[name].Invoke();
        }

        public void Register(string name, Func<IWorkflowMethod> build)
        {
            methods.Add(name, build);
        }
    }
}
