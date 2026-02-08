using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    internal class EfeuEngine : IEfeuEngine
    {
        private readonly IEffectCommands behaviourEffectCommands;

        public EfeuEngine(IEffectCommands behaviourEffectCommands)
        {
            this.behaviourEffectCommands = behaviourEffectCommands;
        }

        public Task SendMessageAsync(EfeuMessage message)
        {
            return behaviourEffectCommands.SendMessage(message, DateTime.Now);
        }
    }
}
