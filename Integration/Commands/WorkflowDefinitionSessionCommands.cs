using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class WorkflowDefinitionSessionCommands
    {
        public WorkflowDefinitionSessionCommands() 
        { 
        
        }

        public Task<int> CreateAsync(int workflowDefinitionId, string userId)
        {
            return Task.FromResult(42);
        }

        public Task SaveAsync(int id)
        {
            // move session data to definition json
            // delete session
            return Task.CompletedTask;
        }

        public Task CancelAsync(int id)
        {
            // delete session
            return Task.CompletedTask;
        }

        public Task TakeOverAsync(int id, string userId)
        {
            // switch session owner if it is stale
            return Task.CompletedTask;
        }


    }
}
