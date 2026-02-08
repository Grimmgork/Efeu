using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Efeu.Application.Controllers
{
    [Route("Trigger")]
    public class TriggerController : Controller
    {
        private readonly ITriggerQueries triggerQueries;
        private readonly ITriggerCommands triggerCommands;

        public TriggerController(ITriggerQueries triggerQueries, ITriggerCommands triggerCommands)
        {
            this.triggerQueries = triggerQueries;
            this.triggerCommands = triggerCommands;
        }

        public async Task<IActionResult> Index()
        {
            TriggerEntity[] triggers = await triggerQueries.GetAllAsync();
            return View(triggers);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await triggerCommands.DetatchAsync([id]);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }
    }
}
