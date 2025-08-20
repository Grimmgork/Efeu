using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public interface IWorkflowMethodProvider
    {
        public IWorkflowMethod GetMethod(string name);
    }
}
