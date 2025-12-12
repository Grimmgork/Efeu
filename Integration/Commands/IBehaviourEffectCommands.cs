using Efeu.Integration.Entities;
using Efeu.Router;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourEffectCommands
    {
        public Task SkipEffect(int id, EfeuValue output = default);

        public Task CreateEffect(EfeuMessage message, DateTimeOffset timestamp);

        public BehaviourEffectEntity GetEffectFromMessage(EfeuMessage message, DateTimeOffset timestamp);

        public Task NudgeEffect(int id);

        public Task DeleteAsync(int id);
    }
}
