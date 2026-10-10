using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Efeu.Application.Models;
using Efeu.Integration;
using Efeu.Integration.Utils.Serialization;
using Efeu.Runtime;
using Efeu.Runtime.Value;

namespace Efeu.Application.Controllers;

[Route("Trigger")]
public class TriggerController : Controller
{
    private readonly ITriggerQueries triggerQueries;
    private readonly ITriggerCommands triggerCommands;
    private readonly IBehaviourQueries behaviourQueries;
    private readonly ValueNodeCommands valueNodeCommands;

    public TriggerController(ITriggerQueries triggerQueries, ITriggerCommands triggerCommands, IBehaviourQueries behaviourQueries, IBehaviourScopeQueries behaviourScopeQueries, ValueNodeCommands valueNodeCommands)
    {
        this.triggerQueries = triggerQueries;
        this.triggerCommands = triggerCommands;
        this.behaviourQueries = behaviourQueries;
        this.valueNodeCommands = valueNodeCommands;
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

        EfeuValue[] values = await valueNodeCommands.ReadAsync([triggerEntity.Input, triggerEntity.Scope]);
        EfeuBehaviourStep step = behaviourVersionEntity.GetPosition(triggerEntity.Position);
        
        EfeuTrigger trigger = triggerEntity.MapToEfeuTrigger(step, values[0], values[1]);
        TriggerDetailsViewModel viewModel = new TriggerDetailsViewModel()
        {
            Trigger = trigger,
        };

        return View(viewModel);
    }
}
