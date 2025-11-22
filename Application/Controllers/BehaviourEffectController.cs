using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Runtime.JSON.Converters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Effect")]
    public class BehaviourEffectController : Controller
    {
        private readonly IBehaviourEffectCommands behaviourEffectCommands;

        public BehaviourEffectController(IBehaviourEffectCommands behaviourEffectCommands)
        {
           this.behaviourEffectCommands = behaviourEffectCommands;
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
            await behaviourEffectCommands.CreateEffect(message);
            return Ok();
        }

        [HttpPost]
        [Route("{id}/Run")]
        public async Task<IActionResult> Create(int id)
        {
            await behaviourEffectCommands.RunEffect(id);
            return Ok();
        }
    }
}
