using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Router;
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
        private readonly IBehaviourEffectCommands behaviourEffectCommands;
        private readonly IUnitOfWork unitOfWork;

        public EfeuEngine(IBehaviourEffectCommands behaviourEffectCommands, IUnitOfWork unitOfWork)
        {
            this.behaviourEffectCommands = behaviourEffectCommands;
            this.unitOfWork = unitOfWork;
        }

        public IUnitOfWork UnitOfWork => unitOfWork;

        public Task SendSignalAsync(EfeuMessage message, DateTimeOffset timestamp, string deduplication)
        {
            return behaviourEffectCommands.ProcessSignal(message, deduplication, timestamp, 0);
        }
    }
}
