using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using Efeu.Router;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourDefinitionCommands
    {
        public Task<int> PublishVersionAsync(int definitionId, BehaviourDefinitionStep[] steps);

        public Task<int> CreateAsync(string name);

        public Task DeleteAsync(int id);
    }
}
