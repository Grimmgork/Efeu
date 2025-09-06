using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("WorkflowDefinition")]
    public class WorkflowDefinitionController : Controller
    {
        private readonly IWorkflowDefinitionCommands workflowDefinitionCommands;
        private readonly IWorkflowDefinitionRepository workflowDefinitionRepository;

        public WorkflowDefinitionController(IWorkflowDefinitionCommands workflowDefinitionCommands, IWorkflowDefinitionRepository workflowDefinitionRepository)
        {
            this.workflowDefinitionCommands = workflowDefinitionCommands;
            this.workflowDefinitionRepository = workflowDefinitionRepository;
        }

        [HttpGet]
        [Route("")]
        public Task<WorkflowDefinitionEntity[]> GetAll()
        {
            return workflowDefinitionRepository.GetAllAsync();
        }

        [HttpPost]
        [Route("{name}")]
        public Task Create(string name)
        {
            return workflowDefinitionCommands.CreateAsync(name);
        }

        [HttpGet]
        [Route("{name}")]
        public Task<WorkflowDefinitionEntity> GetByName(string name)
        {
            return workflowDefinitionRepository.GetByNameAsync(name);
        }

        [HttpPost]
        [Route("{id}/Publish")]
        public Task Publish(int id)
        {
            return workflowDefinitionCommands.Publish(id);
        }

        [HttpPatch]
        [Route("{id}/Save")]
        public Task Save(int id, [FromBody] WorkflowDefinition definition)
        {
            return workflowDefinitionCommands.UpdateDefinitionAsync(id, definition);
        }

        [HttpPatch]
        [Route("{id}/Rename")]
        public Task Save(int id, [FromQuery] string name)
        {
            return workflowDefinitionCommands.RenameAsync(id, name);
        }
    }
}
