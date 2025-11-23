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
    public class DefinitionController : Controller
    {
        private readonly IBehaviourDefinitionCommands workflowDefinitionCommands;
        private readonly IBehaviourDefinitionRepository workflowDefinitionRepository;

        public DefinitionController(IBehaviourDefinitionCommands workflowDefinitionCommands, IBehaviourDefinitionRepository workflowDefinitionRepository)
        {
            this.workflowDefinitionCommands = workflowDefinitionCommands;
            this.workflowDefinitionRepository = workflowDefinitionRepository;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            BehaviourDefinitionEntity[] definitions = await workflowDefinitionRepository.GetAllAsync();
            return View(definitions);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> Detail(int id)
        {
            BehaviourDefinitionEntity? definition = await workflowDefinitionRepository.GetByIdAsync(id);
            if (definition == null)
                return NotFound();

            return View(definition);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            await workflowDefinitionCommands.CreateAsync(name);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Publish")]
        public async Task<IActionResult> PublishVersion(IFormFile file, int id)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.IncludeFields = true;
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            BehaviourDefinitionStep[] steps = JsonSerializer.Deserialize<BehaviourDefinitionStep[]>(file.OpenReadStream(), options);
            await workflowDefinitionCommands.PublishVersionAsync(id, steps);

            Response.Headers["HX-Redirect"] = Url.Action($"{id}");
            return Ok();
        }
    }
}
