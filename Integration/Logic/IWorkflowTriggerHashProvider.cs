using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Logic
{
    public interface IWorkflowTriggerHashProvider
    {
        public string GetTriggerHash(object signal);
    }
}
