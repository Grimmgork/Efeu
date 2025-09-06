using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("WorkflowInstance")]
    public class WorkflowInstanceController : Controller
    {

        private readonly IWorkflowInstanceCommands workflowInstanceCommands;
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;

        public WorkflowInstanceController(IWorkflowInstanceCommands workflowInstanceCommands, IWorkflowInstanceRepository workflowInstanceRepository)
        {
            this.workflowInstanceCommands = workflowInstanceCommands;
            this.workflowInstanceRepository = workflowInstanceRepository;
        }

        [HttpGet]
        [Route("")]
        public Task<IEnumerable<WorkflowInstanceEntity>> GetAll()
        {
            return workflowInstanceRepository.GetAllActiveAsync();
        }

        [HttpGet]
        [Route("{id}")]
        public Task<WorkflowInstanceEntity> GetById(int id)
        {
            return workflowInstanceRepository.GetByIdAsync(id);
        }

        [HttpPost]
        [Route("/Execute/{definitionId}")]
        public Task<WorkflowExecutionResult> Execute(int definitionId)
        {
            return workflowInstanceCommands.ExecuteAsync(definitionId, default);
        }

        [HttpPatch]
        [Route("/{id}/Step")]
        public Task<WorkflowExecutionResult> Step(int id)
        {
            return workflowInstanceCommands.StepAsync(id);
        }

        [HttpPatch]
        [Route("/{id}/Continue")]
        public Task<WorkflowExecutionResult> Continue(int id)
        {
            return workflowInstanceCommands.ContinueAsync(id);
        }
    }
}
