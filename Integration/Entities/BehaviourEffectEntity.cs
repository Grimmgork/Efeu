using Efeu.Router;
using Efeu.Runtime.Data;
using System;

namespace Efeu.Integration.Entities
{
    public class BehaviourEffectEntity
    {
        public int Id;

        public string Name = "";

        public Guid TriggerId; // response trigger

        public Guid CorrelationId; // from wich it came

        public EfeuValue Data;

        public DateTimeOffset CreationTime;

        public DateTimeOffset CompletionTime;

        public BehaviourEffectState State;
    }

    public enum BehaviourEffectState
    {
        Running,
        Suspended,
        Error,
        Completed,
    }
}
