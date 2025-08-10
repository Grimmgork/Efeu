using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal interface IWorkflowEngineCommands
    {
        public async Task SendSignal(object signal)
        {
            // get all trigger types wich derive from this signal.
            // get the payload from all of them.
            // try match them all in database.
            // send signal to all workflows for all matches.
        }
    }
}
