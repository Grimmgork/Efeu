using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Efeu.Router;

namespace Efeu.Integration.Commands
{
    internal class BehaviourDefinitionCommands : IBehaviourDefinitionCommands
    {
        private readonly IEfeuUnitOfWork unitOfWork;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;
        private readonly IBehaviourEffectCommands behaviourEffectCommands;
        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;

        public BehaviourDefinitionCommands(IEfeuUnitOfWork unitOfWork, IBehaviourDefinitionRepository repository, IBehaviourEffectCommands behaviourEffectCommands, IBehaviourTriggerCommands behaviourTriggerCommands)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourDefinitionRepository = repository;
            this.behaviourEffectCommands = behaviourEffectCommands;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
        }

        public Task<int> CreateAsync(string name)
        {
            BehaviourDefinitionEntity definition = new BehaviourDefinitionEntity()
            {
                Name = name,
                Version = 0
            };

            return behaviourDefinitionRepository.CreateAsync(definition);
        }

        public Task DeleteAsync(int id)
        {
            return behaviourDefinitionRepository.DeleteAsync(id);
        }

        public async Task<int> PublishVersionAsync(int definitionId, BehaviourDefinitionStep[] steps)
        {
            unitOfWork.EnsureTransaction();

            BehaviourDefinitionVersionEntity? definitionVersionEntity = await behaviourDefinitionRepository.GetLatestVersionAsync(definitionId);
            if (definitionVersionEntity != null)
            {
                // clear all static triggers for old definition
                await behaviourTriggerCommands.DetatchStaticAsync(definitionVersionEntity.Id);
            }

            // create new version
            int newDefinitionVersionId = await CreateVersionAsync(definitionId, steps);

            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), newDefinitionVersionId);

            DateTimeOffset timestamp = DateTime.Now;
            await behaviourTriggerCommands.AttachAsync(runtime.Triggers.ToArray(), timestamp);
            await behaviourEffectCommands.CreateEffectsBulk(runtime.Messages.ToArray(), timestamp);

            return newDefinitionVersionId;
        }

        private async Task<int> CreateVersionAsync(int definitionId, BehaviourDefinitionStep[] steps)
        {
            BehaviourDefinitionEntity? definition = await behaviourDefinitionRepository.GetByIdAsync(definitionId);
            if (definition == null)
                throw new Exception();

            definition.Version++;
            await behaviourDefinitionRepository.UpdateAsync(definition);

            BehaviourDefinitionVersionEntity definitionVersion = new BehaviourDefinitionVersionEntity()
            {
                DefinitionId = definitionId,
                Steps = steps,
                Version = definition.Version,
            };

            return await behaviourDefinitionRepository.CreateVersionAsync(definitionVersion);
        }
    }
}
