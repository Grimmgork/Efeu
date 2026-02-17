using Efeu.Integration.Commands;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Efeu.Application.Controllers
{
    [Route("Message")]
    public class MessageController : Controller
    {
        private readonly IEffectCommands effectCommands;

        public MessageController(IEffectCommands effectCommands)
        {
            this.effectCommands = effectCommands;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Create([FromBody]EfeuMessage message)
        {
            await effectCommands.SendMessage(message);
            return Ok();
        }
    }
}
