using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class BehaviourDefinitionSessionCommands
    {
        public BehaviourDefinitionSessionCommands()
        {

        }

        public Task Save(Guid sessionId, DateTimeOffset timestamp)
        {
            return Task.CompletedTask;
        }

        public Task SaveAndEnd(Guid sessionId, DateTimeOffset timestamp)
        {
            return Task.CompletedTask;
        }

        public Task Cancel(Guid sessionId, DateTimeOffset timestamp)
        {
            return Task.CompletedTask;
        }

        public Task<BehaviourDefinitionStep> AddElseStep(Guid sessionId, DateTimeOffset timestamp, Guid stepId)
        {
            throw new Exception();
        }

        public Task<BehaviourDefinitionStep> AddDoStep(Guid sessionId, DateTimeOffset timestamp, Guid stepId)
        {
            throw new Exception();
        }

        public Task DeleteStep()
        {
            return Task.CompletedTask;
        }

        public Task GetById(string id)
        {
            return Task.CompletedTask;
        }
    }
}
