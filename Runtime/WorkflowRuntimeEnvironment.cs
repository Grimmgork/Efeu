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

        public WorkflowRuntimeEnvironment(IWorkflowMethodProvider methodProvider, IWorkflowFunctionProvider functionProvider, IWorkflowTriggerProvider triggerProvider)
        {
            MethodProvider = methodProvider;
            FunctionProvider = functionProvider;
            TriggerProvider = triggerProvider;
        }
    }
}
