using Efeu.Integration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class WorkflowDefinitionCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWorkflowDefinitionRepository repository;

        public WorkflowDefinitionCommands(IUnitOfWork unitOfWork, IWorkflowDefinitionRepository repository)
        {
            this.unitOfWork = unitOfWork;
            this.repository = repository;
        }

        public async Task Create(string name)
        {
            string trimmedName = name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new Exception("Name must not be empty.");

            WorkflowDefinitionEntity definition = new WorkflowDefinitionEntity()
            {
                Name = trimmedName
            };

            await repository.CreateAsync(definition);
        }

        public async Task Delete(int id)
        {
            await repository.DeleteAsync(id);
        }
    }
}
