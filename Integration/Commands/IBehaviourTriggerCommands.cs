using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal interface IBehaviourTriggerCommands
    {
        public Task AttachAsync(EfeuTrigger[] triggers, DateTimeOffset timestamp);

        public Task DetatchStaticAsync(int definitionVersionId);

        public Task DetatchAsync(Guid[] ids);

        public Task ResolveMattersAsync(Guid[] matters);
    }
}
