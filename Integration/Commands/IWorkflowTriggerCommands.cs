using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IWorkflowTriggerCommands
    {
        public Task SendSignal(object signal)
        {
            // get trigger hash
            // get all instances matching the trigger
            // run all instances mathcing the trigger

            throw new Exception();
        }
    }
}
