using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourEffectCommands
    {
        public Task RunEffect(int id);

        public Task CreateEffect(EfeuMessage message);

        public Task PublishAsync(int definitionId, BehaviourDefinitionStep[] steps);

        public Task DeleteAsync(int id);
    }
}
