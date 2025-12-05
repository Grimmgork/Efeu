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
        private readonly IUnitOfWork unitOfWork;

        public DefinitionController(IBehaviourDefinitionCommands workflowDefinitionCommands, IBehaviourDefinitionRepository workflowDefinitionRepository, IUnitOfWork unitOfWork)
        {
            this.workflowDefinitionCommands = workflowDefinitionCommands;
            this.workflowDefinitionRepository = workflowDefinitionRepository;
            this.unitOfWork = unitOfWork;
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

        [HttpGet]
        [Route("{id}/Latest")]
        public async Task<ActionResult> Latest(int id)
        {
            BehaviourDefinitionVersionEntity? definition = await workflowDefinitionRepository.GetLatestVersionAsync(id);
            if (definition == null)
                return NotFound();

            return View(definition);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            await workflowDefinitionCommands.CreateAsync(name);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await workflowDefinitionCommands.DeleteAsync(id);
            Response.Headers["HX-Refresh"] = "true";
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

            await unitOfWork.BeginAsync();
            await workflowDefinitionCommands.PublishVersionAsync(id, steps);
            await unitOfWork.CommitAsync();

            Response.Headers["HX-Redirect"] = Url.Action($"{id}");
            return Ok();
        }
    }
}
