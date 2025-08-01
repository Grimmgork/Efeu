using Efeu.Integration.Commands;
using Efeu.Integration.Data;
using Efeu.Integration.Model;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Json;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    public class EditorController : Controller
    {
        private readonly IWorkflowInstanceRepository workflowInstanceRepository;

        public EditorController(IWorkflowInstanceRepository workflowInstanceRepository, IWorkflowInstanceCommands workflowInstanceCommands)
        {
            this.workflowInstanceRepository = workflowInstanceRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("ImportFile")]
        public IActionResult ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return this.BadRequest();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new SomeDataJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            try
            {
                WorkflowDefinition definition = JsonSerializer.Deserialize<WorkflowDefinition>(file.OpenReadStream(), options);

                return PartialView("Editor", definition);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("WorkflowInstance")]
        public async Task CreateWorkflowInstance()
        {
            WorkflowInstanceEntity entity = new WorkflowInstanceEntity();
            entity.Input = 42;
            entity.State = WorkflowInstanceState.Running;
            entity.Variables = new SomeStruct();
            entity.Variables["Variable"] = 13;
            await workflowInstanceRepository.Add(entity);
        }

        [HttpGet]
        [Route("WorkflowInstance/{id}")]
        public async Task<WorkflowInstanceEntity> GetWorkflowInstance(int id)
        {
            WorkflowInstanceEntity entity = await workflowInstanceRepository.GetById(id);
            return entity;
        }
    }
}
