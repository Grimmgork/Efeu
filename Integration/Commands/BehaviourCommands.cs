using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Efeu.Runtime;

namespace Efeu.Integration.Commands
{
    internal class BehaviourCommands : IBehaviourCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IBehaviourQueries behaviourQueries;
        private readonly IEffectCommands effectCommands;
        private readonly ITriggerCommands triggerCommands;

        public BehaviourCommands(IEfeuUnitOfWork unitOfWork, IBehaviourQueries behaviourQueries, IEffectCommands effectCommands, ITriggerCommands triggerCommands)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourQueries = behaviourQueries;
            this.effectCommands = effectCommands;
            this.triggerCommands = triggerCommands;
        }

        public Task<int> CreateAsync(string name)
        {
            BehaviourEntity behaviourEntity = new BehaviourEntity()
            {
                Name = name,
                Version = 0
            };

            return behaviourQueries.CreateAsync(behaviourEntity);
        }

        public Task DeleteAsync(int id)
        {
            return behaviourQueries.DeleteAsync(id);
        }

        public async Task<int> PublishVersionAsync(int behaviourId, EfeuBehaviourStep[] steps)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync($"Definition:{behaviourId}");
            BehaviourVersionEntity? behaviourVersionEntity = await behaviourQueries.GetLatestVersionAsync(behaviourId);
            if (behaviourVersionEntity != null)
            {
                // clear all static triggers for old definition
                await triggerCommands.DetatchStaticAsync(behaviourVersionEntity.Id);
            }

            int newBehaviourVersionId = await CreateVersionAsync(behaviourId, steps);

            await effectCommands.RunImmediate(steps, newBehaviourVersionId, DateTime.Now);
            await unitOfWork.CompleteAsync();
            return newBehaviourVersionId;
        }

        private async Task<int> CreateVersionAsync(int behaviourId, EfeuBehaviourStep[] steps)
        {
            BehaviourEntity? behaviourEntity = await behaviourQueries.GetByIdAsync(behaviourId);
            if (behaviourEntity == null)
                throw new Exception();

            behaviourEntity.Version++;
            await behaviourQueries.UpdateAsync(behaviourEntity);

            BehaviourVersionEntity behaviourVersionEntity = new BehaviourVersionEntity()
            {
                BehaviourId = behaviourId,
                Steps = steps,
                Version = behaviourEntity.Version,
            };

            return await behaviourQueries.CreateVersionAsync(behaviourVersionEntity);
        }
    }
}
