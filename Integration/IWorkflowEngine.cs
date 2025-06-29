using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Signal;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System.Threading;

namespace Efeu.Integration
{
    public interface IWorkflowEngine
    {
        public Task<SomeStruct> ExecuteWorkflowAsync(WorkflowDefinition definition, SomeStruct input, CancellationToken token = default);
    }
}
