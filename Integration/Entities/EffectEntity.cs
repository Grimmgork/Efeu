using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using Efeu.Integration.Utils.Serialization;

namespace Efeu.Integration.Entities;

public enum EffectState
{
    Running,
    Suspended,
    Faulted
}

public class EffectEntity
{
    public Guid Id; // guid similar to message id

    public string Type = "";

    public Guid CorrelationId; // from wich it came

    public EfeuValueHash Data;

    public EfeuValueHash Input;

    public string Fault = "";

    public DateTimeOffset CreationTime;

    public DateTimeOffset ExecutionTime;

    public uint Times;

    public EfeuMessageTag Tag;

    public EffectState State;

    public Guid Matter;


    public DateTimeOffset LockedUntil;

    public Guid LockId;
}
