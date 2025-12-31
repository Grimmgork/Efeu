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
        public Task SkipEffect(int id, DateTimeOffset timestamp, EfeuValue output = default);

        public Task CreateEffect(EfeuMessage message, DateTimeOffset timestamp);

        public Task CreateEffectsBulk(EfeuMessage[] messages, DateTimeOffset timestamp);

        public Task SuspendEffect(int id, DateTimeOffset timestamp);

        public Task NudgeEffect(int id);

        public Task DeleteEffect(int id);

        public Task ProcessSignal(EfeuMessage message, int effectId);
    }
}
