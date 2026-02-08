using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourCommands
    {
        public Task<int> CreateAsync(string name);

        public Task<int> PublishVersionAsync(int behaviourId, EfeuBehaviourStep[] steps);

        public Task DeleteAsync(int id);
    }
}
