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

        public Task ProcessSignalAsync(EfeuSignal signal)
        {
            EfeuMessage message = new EfeuMessage()
            {
                Tag = EfeuMessageTag.Incoming,
                Name = signal.Name,
                Data = signal.Data,
                TriggerId = signal.TriggerId,
            };
            return behaviourEffectCommands.ProcessSignal(message, signal.Id, signal.Timestamp);
        }

        public Task ClearDeduplicationKeysBeforeAsync(DateTimeOffset before)
        {
            return deduplicationStore.ClearBeforeAsync(before);
        }
    }
}
