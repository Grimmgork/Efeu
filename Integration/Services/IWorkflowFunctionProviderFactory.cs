using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Services
{
    public interface IWorkflowFunctionProviderFactory
    {
        public IWorkflowFunctionProvider CreateAsync();
    }
}
