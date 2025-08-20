using Efeu.Runtime.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public interface IWorkflowFunctionProvider
    {
        public IWorkflowFunction GetFunction(string name);
    }
}
