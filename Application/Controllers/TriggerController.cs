using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Efeu.Application.Models;
using Efeu.Integration;
using Efeu.Runtime;

namespace Efeu.Application.Controllers
{
    [Route("Trigger")]
    public class TriggerController : Controller
    {
        private readonly ITriggerQueries triggerQueries;
        private readonly ITriggerCommands triggerCommands;
        private readonly IBehaviourQueries behaviourQueries;
        private readonly IBehaviourScopeQueries  behaviourScopeQueries;

        public TriggerController(ITriggerQueries triggerQueries, ITriggerCommands triggerCommands, IBehaviourQueries behaviourQueries, IBehaviourScopeQueries behaviourScopeQueries)
        {
            this.triggerQueries = triggerQueries;
            this.triggerCommands = triggerCommands;
            this.behaviourQueries = behaviourQueries;
            this.behaviourScopeQueries = behaviourScopeQueries;
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
            await triggerCommands.DeleteAsync([id]);
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            TriggerEntity? triggerEntity = await triggerQueries.GetByIdAsync(id);
            if (triggerEntity == null)
            {
                return NotFound();
            }
            
            BehaviourVersionEntity? behaviourVersionEntity = await behaviourQueries.GetVersionByIdAsync(triggerEntity.BehaviourVersionId);
            if (behaviourVersionEntity == null)
            {
                return NotFound();
            }
            
            BehaviourScopeEntity? behaviourScopeEntity = await behaviourScopeQueries.GetByIdAsync(triggerEntity.ScopeId);
            if (behaviourScopeEntity == null)
            {
                return NotFound();
            }
            
            EfeuTrigger trigger = triggerEntity.MapToEfeuTrigger(behaviourVersionEntity.GetPosition(triggerEntity.Position), behaviourScopeEntity.MapToEfeuRuntimeScope());
            TriggerDetailsViewModel viewModel = new TriggerDetailsViewModel()
            {
                Trigger = trigger,
            };
            
            return View(viewModel);
        }
    }
}
