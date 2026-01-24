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
        private readonly IBehaviourEffectCommands behaviourEffectCommands;
        private readonly IDeduplicationKeyRepository deduplicationStore;
        private readonly IEfeuUnitOfWork unitOfWork;

        public EfeuEngine(IBehaviourEffectCommands behaviourEffectCommands, IDeduplicationKeyRepository deduplicationStore, IEfeuUnitOfWork unitOfWork)
        {
            this.behaviourEffectCommands = behaviourEffectCommands;
            this.deduplicationStore = deduplicationStore;
            this.unitOfWork = unitOfWork;
        }

        public IEfeuUnitOfWork UnitOfWork => unitOfWork;

        public Task SendMessageAsync(EfeuMessage message)
        {
            return behaviourEffectCommands.SendMessage(message, DateTime.Now);
        }

        public Task CreateEffectAsync(EfeuMessage message)
        {
            return behaviourEffectCommands.CreateEffect(message);
        }

        public Task ClearDeduplicationKeysBeforeAsync(DateTimeOffset before)
        {
            return deduplicationStore.ClearBeforeAsync(before);
        }
    }
}
