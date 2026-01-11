using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class BehaviourTriggerEntity
    {
        public Guid Id;

        public DateTimeOffset CreationTime;

        public int DefinitionVersionId;

        public EfeuValue Input;

        public Guid CorrelationId;

        public string Position = ""; // position of trigger row 0/Else/1

        public EfeuRuntimeScope Scope = new EfeuRuntimeScope(); // Scope around trigger row

        public string Name = "";

        public EfeuMessageTag Tag;

        // public bool IsManaged; flag to indicate whether or not it is a IEfeuEffect implementation

        public int EffectId; // effect to resume
    }
}
