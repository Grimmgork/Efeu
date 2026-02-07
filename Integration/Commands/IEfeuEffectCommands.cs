using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IEfeuEffectCommands
    {
        public Task SkipEffect(Guid id, DateTimeOffset timestamp, EfeuValue output = default);

        public Task SuspendEffect(Guid id, DateTimeOffset timestamp);

        public Task NudgeEffect(Guid id);

        public Task AbortEffect(Guid id);

        public Task CreateEffect(EfeuMessage message);

        public Task SendMessage(EfeuMessage message, DateTimeOffset timestamp);

        public Task RunImmediate(BehaviourDefinitionStep[] steps, int definitionVersionId, DateTimeOffset timestamp);
    }
}
