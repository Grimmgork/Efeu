using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Runtime.JSON.Converters;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Definition")]
    public class BehaviourDefinitionController : Controller
    {
        private readonly IBehaviourDefinitionCommands workflowDefinitionCommands;
        private readonly IBehaviourDefinitionRepository workflowDefinitionRepository;

        public BehaviourDefinitionController(IBehaviourDefinitionCommands workflowDefinitionCommands, IBehaviourDefinitionRepository workflowDefinitionRepository)
        {
            this.workflowDefinitionCommands = workflowDefinitionCommands;
            this.workflowDefinitionRepository = workflowDefinitionRepository;
        }

        [HttpGet]
        [Route("")]
        public Task<BehaviourDefinitionEntity[]> GetAll()
        {
            return workflowDefinitionRepository.GetAllAsync();
        }

        [HttpPost]
        public IActionResult Create(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            BehaviourDefinitionVersionEntity definition = JsonSerializer.Deserialize<BehaviourDefinitionVersionEntity>(file.OpenReadStream(), options);
            workflowDefinitionCommands.CreateVersionAsync(definition);
            return Ok();
        }

        [HttpGet]
        [Route("{name}")]
        public Task<BehaviourDefinitionVersionEntity> GetByName(string name)
        {
            return workflowDefinitionRepository.GetNewestVersionAsync(name);
        }
    }
}
