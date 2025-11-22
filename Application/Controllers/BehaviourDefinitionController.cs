using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Runtime.JSON.Converters;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity.Core.Common.CommandTrees;
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
        [Route("")]
        public async Task<IActionResult> Create(string name)
        {
            await workflowDefinitionCommands.CreateAsync(name);
            return Ok();
        }

        [HttpPost]
        [Route("{definitionId}/Publish")]
        public async Task<IActionResult> PublishVersion(IFormFile file, int definitionId)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.IncludeFields = true;
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            BehaviourDefinitionStep[] steps = JsonSerializer.Deserialize<BehaviourDefinitionStep[]>(file.OpenReadStream(), options);
            await workflowDefinitionCommands.PublishVersionAsync(definitionId, steps);
            return Ok();
        }
    }
}
