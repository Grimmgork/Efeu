using Efeu.Integration.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public interface IWorkflowTriggerProvider
    {
        public IWorkflowTrigger GetTrigger(string name);
    }
}
