using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Json.Converters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Behaviour")]
    public class BehaviourController : Controller
    {
        private readonly IBehaviourCommands behaviourCommands;
        private readonly IBehaviourQueries behaviourQueries;
        private readonly IEfeuUnitOfWork unitOfWork;

        public BehaviourController(IBehaviourCommands behaviourCommands, IBehaviourQueries behaviourQueries, IEfeuUnitOfWork unitOfWork)
        {
            this.behaviourCommands = behaviourCommands;
            this.behaviourQueries = behaviourQueries;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            BehaviourEntity[] behaviours = await behaviourQueries.GetAllAsync();
            return View(behaviours);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> Detail(int id)
        {
            BehaviourEntity? behaviour = await behaviourQueries.GetByIdAsync(id);
            if (behaviour == null)
                return NotFound();

            return View(behaviour);
        }

        [HttpGet]
        [Route("{id}/Latest")]
        public async Task<ActionResult> Latest(int id)
        {
            BehaviourVersionEntity? behaviour = await behaviourQueries.GetLatestVersionAsync(id);
            if (behaviour == null)
                return NotFound();

            return View(behaviour);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            await behaviourCommands.CreateAsync(name);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await behaviourCommands.DeleteAsync(id);
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

            EfeuBehaviourStep[] steps = JsonSerializer.Deserialize<EfeuBehaviourStep[]>(file.OpenReadStream(), options);

            await behaviourCommands.PublishVersionAsync(id, steps);

            Response.Headers["HX-Redirect"] = Url.Action($"{id}");
            return Ok();
        }
    }
}
