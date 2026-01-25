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
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;

        public TriggerController(IBehaviourTriggerRepository workflowInstanceRepository, IBehaviourTriggerCommands behaviourTriggerCommands)
        {
            this.behaviourTriggerRepository = workflowInstanceRepository;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
        }

        public async Task<IActionResult> Index()
        {
            BehaviourTriggerEntity[] triggers = await behaviourTriggerRepository.GetAllAsync();
            return View(triggers);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await behaviourTriggerCommands.DetatchAsync([id]);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }
    }
}
