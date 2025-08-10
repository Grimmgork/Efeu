using Efeu.Integration.Data;
using Efeu.Integration.Model;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class WorkflowDefinitionCommands : IWorkflowDefinitionCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWorkflowDefinitionRepository definitionRepository;
        private readonly IWorkflowDefinitionVersionRepository versionRepository;

        public WorkflowDefinitionCommands(IUnitOfWork unitOfWork, IWorkflowDefinitionRepository repository, IWorkflowDefinitionVersionRepository versionRepository)
        {
            this.unitOfWork = unitOfWork;
            this.definitionRepository = repository;
            this.versionRepository = versionRepository;
        }

        public async Task CreateAsync(string name)
        {
            name = name.Trim();
            ValidateDefinitionName(name);
                
            WorkflowDefinitionEntity definition = new WorkflowDefinitionEntity()
            {
                Name = name
            };

            await definitionRepository.CreateAsync(definition);
        }

        public async Task Delete(int id)
        {
            await definitionRepository.DeleteAsync(id);
        }

        public Task RollbackToVersion(int id, int version)
        {
            return unitOfWork.ExecuteAsync(IsolationLevel.Serializable, async () =>
            {
                WorkflowDefinitionVersionEntity definitionVersion = await versionRepository.GetByVersionAsync(id, version);
                WorkflowDefinitionEntity definition = await definitionRepository.GetByIdAsync(id);
                definition.Definition = definitionVersion.Definition;
                await definitionRepository.UpdateAsync(definition);
            });
        }

        public Task Publish(int id)
        {
            return unitOfWork.ExecuteAsync(IsolationLevel.Serializable, async () =>
            {
                WorkflowDefinitionEntity definitionEntity = await definitionRepository.GetByIdAsync(id);
                WorkflowDefinitionVersionEntity latestVersion = await versionRepository.GetLatestVersion(id);

                WorkflowDefinitionVersionEntity version = new WorkflowDefinitionVersionEntity()
                {
                    Created = DateTime.Now,
                    Version = latestVersion.Version + 1,
                    WorkflowDefinitionId = definitionEntity.Id,
                    Definition = definitionEntity.Definition
                };
            });
        }

        public async Task UpdateDefinitionAsync(int id, WorkflowDefinition definition)
        {
            WorkflowDefinitionEntity definitionEntity = await definitionRepository.GetByIdAsync(id);
            definitionEntity.Definition = definition; // TODO Validate / compile definition
            await definitionRepository.UpdateAsync(definitionEntity);
        }

        public async Task RenameAsync(int id, string name)
        {
            WorkflowDefinitionEntity definitionEntity = await definitionRepository.GetByIdAsync(id);
            name = name.Trim();
            ValidateDefinitionName(name);
            definitionEntity.Name = name;
            await definitionRepository.UpdateAsync(definitionEntity);
        }

        private void ValidateDefinitionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Name must not be empty.");

            if (!Regex.IsMatch(name, "^[a-zA-Z0-9._-]+$"))
                throw new Exception("Invalid character in name.");
        }
    }
}
