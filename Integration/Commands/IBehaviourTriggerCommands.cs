using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal interface IBehaviourTriggerCommands
    {
        public Task CreateAsync(BehaviourTrigger trigger);

        public Task CreateBulkAsync(BehaviourTrigger[] triggers);

        public Task DeleteStaticAsync(int definitionVersionId);

        public Task DeleteBulkAsync(Guid[] ids);
    }
}
