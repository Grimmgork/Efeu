using Efeu.Runtime.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Logic
{
    public interface IWorkflowTrigger
    {
        public WorkflowTrigger GetTrigger();
    }
}
