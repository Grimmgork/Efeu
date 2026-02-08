using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Script;
using Efeu.Runtime.Value;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Effect")]
    public class EffectController : Controller
    {
        private readonly IEffectCommands effectCommands;
        private readonly IEffectQueries effectQueries;
        private readonly JsonOptions jsonOptions;

        public EffectController(IEffectCommands effectCommands, IEffectQueries effectQueries, IOptions<JsonOptions> jsonOptions)
        {
            this.effectCommands = effectCommands;
            this.effectQueries = effectQueries;
            this.jsonOptions = jsonOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            EffectEntity[] effects = await effectQueries.GetAllAsync();
            return View(effects);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create(string type, EfeuMessageTag tag, Guid matter, string json)
        {
            await effectCommands.CreateEffect(new EfeuMessage()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Type = type,
                Tag = tag,
                CorrelationId = Guid.NewGuid(),
                Matter = Guid.Empty,
                Data = JsonSerializer.Deserialize<EfeuValue>(string.IsNullOrWhiteSpace(json) ? "null" : json, jsonOptions.JsonSerializerOptions)
            });
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await effectCommands.AbortEffect(id);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Nudge")]
        public async Task<IActionResult> Nudge(Guid id)
        {
            await effectCommands.NudgeEffect(id);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Suspend")]
        public async Task<IActionResult> Suspend(Guid id)
        {
            await effectCommands.SuspendEffect(id, DateTime.Now);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }
    }
}
