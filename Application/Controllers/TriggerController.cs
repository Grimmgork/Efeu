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

        public TriggerController(IBehaviourTriggerRepository workflowInstanceRepository)
        {
            this.behaviourTriggerRepository = workflowInstanceRepository;
        }

        [HttpGet]
        [Route("")]
        public Task<BehaviourTriggerEntity[]> GetAll()
        {
            return behaviourTriggerRepository.GetAllAsync();
        }
    }
}
