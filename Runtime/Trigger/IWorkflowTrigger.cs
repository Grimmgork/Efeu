using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Logic
{
    public interface IWorkflowTrigger
    {
        public Task AttachAsync(WorkflowTriggerContext context);

        public Task ReattachAsync(WorkflowTriggerContext context);

        public Task DetachAsync(WorkflowTriggerContext context);

        public Task Signal(WorkflowTriggerContext context, object signal);
    }
}
