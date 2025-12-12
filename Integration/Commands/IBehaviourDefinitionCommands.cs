using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourDefinitionCommands
    {
        public Task<int> CreateAsync(string name);

        public Task<int> PublishVersionAsync(int definitionId, BehaviourDefinitionStep[] steps);

        public Task DeleteAsync(int id);
    }
}
