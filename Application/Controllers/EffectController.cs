using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Router.Value;
using Efeu.Router.Script;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Effect")]
    public class EffectController : Controller
    {
        private readonly IBehaviourEffectCommands behaviourEffectCommands;
        private readonly IBehaviourEffectRepository behaviourEffectRepository;

        public EffectController(IBehaviourEffectCommands behaviourEffectCommands, IBehaviourEffectRepository behaviourEffectRepository)
        {
            this.behaviourEffectCommands = behaviourEffectCommands;
            this.behaviourEffectRepository = behaviourEffectRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            EfeuEffectEntity[] effects = await behaviourEffectRepository.GetAllAsync();
            return View(effects);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create(string name, EfeuMessageTag tag)
        {
            await behaviourEffectCommands.CreateEffect(new EfeuMessage()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Name = name,
                Tag = tag,
                CorrelationId = Guid.NewGuid(),
            });
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await behaviourEffectCommands.DeleteEffect(id);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Nudge")]
        public async Task<IActionResult> Nudge(Guid id)
        {
            await behaviourEffectCommands.NudgeEffect(id);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Suspend")]
        public async Task<IActionResult> Suspend(Guid id)
        {
            await behaviourEffectCommands.SuspendEffect(id, DateTime.Now);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }
    }
}
