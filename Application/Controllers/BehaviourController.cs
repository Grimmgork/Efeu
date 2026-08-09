using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Json.Converters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Efeu.Integration.Json;
using Efeu.Runtime.Value;

namespace Efeu.Application.Controllers
{
    [Route("Behaviour")]
    public class BehaviourController : Controller
    {
        private readonly IBehaviourCommands behaviourCommands;
        private readonly IBehaviourQueries behaviourQueries;

        public BehaviourController(IBehaviourCommands behaviourCommands, IBehaviourQueries behaviourQueries, IEfeuUnitOfWork unitOfWork)
        {
            this.behaviourCommands = behaviourCommands;
            this.behaviourQueries = behaviourQueries;
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
            BehaviourVersionEntity? behaviourVersion = await behaviourQueries.GetLatestVersionAsync(id);
            if (behaviourVersion == null)
                return NotFound();

            return Redirect($"/Behaviour/Version/{behaviourVersion.Id}");
        }
        
        [HttpGet]
        [Route("Version/{id}")]
        public async Task<ActionResult> Version(Guid id)
        {
            BehaviourVersionEntity? behaviourVersion = await behaviourQueries.GetVersionByIdAsync(id);
            if (behaviourVersion == null)
                return NotFound();
            
            return View(behaviourVersion);
        }
        
        [HttpGet]
        [Route("Version/{id}/Raw")]
        public async Task<ActionResult> VersionRaw(Guid id)
        {
            BehaviourVersionEntity? behaviourVersion = await behaviourQueries.GetVersionByIdAsync(id);
            if (behaviourVersion == null)
                return NotFound();
            
            var resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(
                JsonModifierBuilder.For<EfeuBehaviourStep>()
                    .IgnoreWhenEquals(i => i.Id, Guid.Empty)
                    .IgnoreWhenEmpty(i => i.Do)
                    .IgnoreWhenEmpty(i => i.Else)
                    .IgnoreWhenEmpty(i => i.Error)
                    .IgnoreWhenNull(i => i.ArgumentName)
                    .IgnoreWhenEmpty(i => i.Name)
                    .Build());
            
            resolver.Modifiers.Add(
                JsonModifierBuilder.For<EfeuBehaviourExpression>()
                    .IgnoreWhenEquals(i => i.Type, EfeuExpressionType.Nil)
                    .IgnoreWhenEmpty(i => i.Code)
                    .IgnoreWhenEmpty(i => i.Items)
                    .IgnoreWhenEmpty(i => i.Fields)
                    .IgnoreWhenEquals(i => i.Value, EfeuValue.Nil())
                    .Build());
            
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.IncludeFields = true;
            options.TypeInfoResolver = resolver;
            options.WriteIndented = true;
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            BehaviourExport export = new BehaviourExport()
            {
                Id = behaviourVersion.Id,
                Version = behaviourVersion.Version,
                Steps = behaviourVersion.Steps
            };
            
            return new JsonResult(export, options);
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
            if (file.Length == 0)
                return BadRequest();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.IncludeFields = true;
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            EfeuBehaviourStep[] steps = JsonSerializer.Deserialize<EfeuBehaviourStep[]>(file.OpenReadStream(), options) ?? [];

            await behaviourCommands.PublishVersionAsync(id, steps);

            Response.Headers["HX-Redirect"] = Url.Action($"{id}");
            return Ok();
        }
    }
}
