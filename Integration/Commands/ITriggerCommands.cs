using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface ITriggerCommands
    {
        public Task CreateAsync(EfeuTrigger[] triggers);

        public Task DetatchStaticAsync(int behaviourVersionId);

        public Task DeleteAsync(Guid[] ids);

        public Task ResolveMattersAsync(Guid[] matters);

        public Task CompleteGroupsAsync(Guid[] groups);
    }
}
