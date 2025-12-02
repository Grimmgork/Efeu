using Efeu.Integration.Entities;
using Efeu.Router;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourEffectCommands
    {
        public Task RunEffect(BehaviourEffectEntity effect);

        public Task SkipEffect(BehaviourEffectEntity effect, EfeuValue output = default);

        public Task CreateEffect(EfeuMessage message, DateTimeOffset timestamp);

        public Task NudgeEffect(BehaviourEffectEntity effect);

        public Task DeleteAsync(int id);
    }
}
