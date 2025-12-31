using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IBehaviourSignalCommands
    {
        public Task ProcessSignal(EfeuMessage message);
    }
}
