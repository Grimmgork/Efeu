using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class WorkflowRuntimeEnvironment
    {
        public readonly IWorkflowMethodProvider MethodProvider;

        public readonly IWorkflowFunctionProvider FunctionProvider;

        public readonly IWorkflowTriggerProvider TriggerProvider;

        public readonly IWorkflowOutbox Outbox;

        public WorkflowRuntimeEnvironment(
            IWorkflowMethodProvider methodProvider, 
            IWorkflowFunctionProvider functionProvider, 
            IWorkflowTriggerProvider triggerProvider, 
            IWorkflowOutbox outbox)
        {
            MethodProvider = methodProvider;
            FunctionProvider = functionProvider;
            TriggerProvider = triggerProvider;
            Outbox = outbox;
        }
    }
}
