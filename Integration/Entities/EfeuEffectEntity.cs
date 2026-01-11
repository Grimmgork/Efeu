using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;

namespace Efeu.Integration.Entities
{
    public class EfeuEffectEntity
    {
        public Guid Id; // guid similar to message id

        public string Name = "";

        public Guid TriggerId; // response trigger

        public Guid CorrelationId; // from wich it came

        public EfeuValue Data;

        public EfeuValue Input;

        public string Fault = "";

        public DateTimeOffset CreationTime;

        public DateTimeOffset ExecutionTime;

        public uint Times;

        public EfeuMessageTag Tag;

        public BehaviourEffectState State;

        public DateTimeOffset LockedUntil;

        public Guid LockId;
    }

    public enum BehaviourEffectState
    {
        Running,
        Suspended,
        Faulted,
    }
}
