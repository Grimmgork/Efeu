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

        public EfeuValue Input;

        public DateTimeOffset CreationTime;

        public uint Times;

        public BehaviourEffectTag Tag;

        public DateTimeOffset LockedUntil;

        public Guid LockId;

        public BehaviourEffectState State;
    }

    public enum BehaviourEffectState
    {
        Running,
        Suspended,
        Error
    }

    public enum BehaviourEffectTag
    {
        Signal,
        Effect
    }
}
