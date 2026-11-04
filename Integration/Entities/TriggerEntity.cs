using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Integration.Utils.Serialization;

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

    public EfeuValueHash Input;
    
    public EfeuValueHash Scope;

    public string LoopbackPosition = "";
    
    public EfeuValueHash LoopbackScope;

    public Guid Matter;

    public Guid Group;
}
