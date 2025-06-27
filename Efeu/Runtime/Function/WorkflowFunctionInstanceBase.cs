using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Message;
using Efeu.Runtime.Method;
using Efeu.Runtime.Signal;

namespace Efeu.Runtime.Function
{
    public abstract class WorkflowFunctionInstanceBase : IWorkflowFunctionInstance
    {
        public int Id { get; set; }

        public virtual void Dispose()
        {
            // do nothing
        }

        public virtual SomeDataStruct Run(IWorkflowSignalHandler context, SomeDataStruct input)
        {
            throw new NotImplementedException();
        }

        public SomeDataStruct Run(SomeDataStruct input)
        {
            throw new NotImplementedException();
        }
    }


}
