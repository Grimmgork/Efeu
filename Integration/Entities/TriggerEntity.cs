using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities;

public class TriggerEntity
{
    public Guid Id;

    public DateTimeOffset CreationTime;

    public Guid CorrelationId;

    public Guid BehaviourVersionId;

    public string Position = ""; // position of trigger row 0/Else/1

    public string Type = "";

    public EfeuMessageTag Tag;

    public EfeuValue Input = "";

    // public string Scope = "";

    // public string LoopbackPosition = "";
    
    // public string LoopbackScope = "";

    public Guid Matter;

    public Guid Group;

    public Guid ScopeId;

    public Guid LoopbackScopeId;

    public bool IsDetatched;
}
