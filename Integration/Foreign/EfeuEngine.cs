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
        private readonly IDeduplicationStore deduplicationStore;
        private readonly IEfeuUnitOfWork unitOfWork;

        public EfeuEngine(IBehaviourEffectCommands behaviourEffectCommands, IDeduplicationStore deduplicationStore, IEfeuUnitOfWork unitOfWork)
        {
            this.behaviourEffectCommands = behaviourEffectCommands;
            this.deduplicationStore = deduplicationStore;
            this.unitOfWork = unitOfWork;
        }

        public IEfeuUnitOfWork UnitOfWork => unitOfWork;

        public Task ProcessSignalAsync(EfeuMessage message, Guid messageId, DateTimeOffset timestamp)
        {
            return behaviourEffectCommands.ProcessSignal(message, messageId, timestamp);
        }

        public Task ClearDeduplicationKeysBeforeAsync(DateTimeOffset before)
        {
            return deduplicationStore.ClearBeforeAsync(before);
        }
    }
}
