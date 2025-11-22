using Efeu.Integration.Commands;
using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Efeu.Application.Controllers
{
    [Route("Trigger")]
    public class BehaviourTriggerController : Controller
    {
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;

        public BehaviourTriggerController(IBehaviourTriggerRepository workflowInstanceRepository)
        {
            this.behaviourTriggerRepository = workflowInstanceRepository;
        }

        [HttpGet]
        [Route("")]
        public Task<IEnumerable<BehaviourTriggerEntity>> GetAll()
        {
            return behaviourTriggerRepository.GetAllActiveAsync();
        }
    }
}
