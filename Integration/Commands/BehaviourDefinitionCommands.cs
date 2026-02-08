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
    internal class BehaviourDefinitionCommands : IBehaviourDefinitionCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IBehaviourDefinitionQueries behaviourDefinitionQueries;
        private readonly IEffectCommands effectCommands;
        private readonly ITriggerCommands triggerCommands;

        public BehaviourDefinitionCommands(IEfeuUnitOfWork unitOfWork, IBehaviourDefinitionQueries queries, IEffectCommands effectCommands, ITriggerCommands triggerCommands)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourDefinitionQueries = queries;
            this.effectCommands = effectCommands;
            this.triggerCommands = triggerCommands;
        }

        public Task<int> CreateAsync(string name)
        {
            BehaviourDefinitionEntity definition = new BehaviourDefinitionEntity()
            {
                Name = name,
                Version = 0
            };

            return behaviourDefinitionQueries.CreateAsync(definition);
        }

        public Task DeleteAsync(int id)
        {
            return behaviourDefinitionQueries.DeleteAsync(id);
        }

        public async Task<int> PublishVersionAsync(int definitionId, BehaviourDefinitionStep[] steps)
        {
            await unitOfWork.BeginAsync();
            await unitOfWork.LockAsync($"Definition:{definitionId}");
            BehaviourDefinitionVersionEntity? definitionVersionEntity = await behaviourDefinitionQueries.GetLatestVersionAsync(definitionId);
            if (definitionVersionEntity != null)
            {
                // clear all static triggers for old definition
                await triggerCommands.DetatchStaticAsync(definitionVersionEntity.Id);
            }

            int newDefinitionVersionId = await CreateVersionAsync(definitionId, steps);

            await effectCommands.RunImmediate(steps, newDefinitionVersionId, DateTime.Now);
            await unitOfWork.CompleteAsync();
            return newDefinitionVersionId;
        }

        private async Task<int> CreateVersionAsync(int definitionId, BehaviourDefinitionStep[] steps)
        {
            BehaviourDefinitionEntity? definition = await behaviourDefinitionQueries.GetByIdAsync(definitionId);
            if (definition == null)
                throw new Exception();

            definition.Version++;
            await behaviourDefinitionQueries.UpdateAsync(definition);

            BehaviourDefinitionVersionEntity definitionVersion = new BehaviourDefinitionVersionEntity()
            {
                DefinitionId = definitionId,
                Steps = steps,
                Version = definition.Version,
            };

            return await behaviourDefinitionQueries.CreateVersionAsync(definitionVersion);
        }
    }
}
