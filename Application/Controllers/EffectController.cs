using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Runtime.JSON.Converters;
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
        private readonly IUnitOfWork unitOfWork;

        public EffectController(IBehaviourEffectCommands behaviourEffectCommands, IBehaviourEffectRepository behaviourEffectRepository, IUnitOfWork unitOfWork)
        {
            this.behaviourEffectCommands = behaviourEffectCommands;
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            BehaviourEffectEntity[] effects = await behaviourEffectRepository.GetAll();
            return View(effects);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create(string name, EfeuMessageTag tag)
        {
            EfeuMessage message = new EfeuMessage()
            {
                Name = name,
                Tag = tag
            };
            await unitOfWork.BeginAsync();
            await behaviourEffectCommands.CreateEffect(message, DateTime.Now);
            await unitOfWork.CommitAsync();
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await unitOfWork.BeginAsync();
            await behaviourEffectCommands.DeleteAsync(id);
            await unitOfWork.CommitAsync();
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Nudge")]
        public async Task<IActionResult> Nudge(int id)
        {
            await unitOfWork.BeginAsync();
            BehaviourEffectEntity? effect = await behaviourEffectRepository.GetByIdAsync(id);
            if (effect == null)
                return NotFound();

            await behaviourEffectCommands.NudgeEffect(effect);
            await unitOfWork.CommitAsync();
            Response.Headers["HX-Refresh"] = "true";
            return Ok();
        }
    }
}
