using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;

namespace Workflows.Function
{
    public interface IWorkflowFunctionInstance : IDisposable
    {
        public SomeDataStruct Run(SomeDataStruct input);
    }
}
