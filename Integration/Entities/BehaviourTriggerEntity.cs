using Efeu.Integration.Model;
using Efeu.Router;
using Efeu.Runtime;
using Efeu.Runtime.Data;
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

        public int DefinitionVersionId;

        public Guid CorrelationId;

        public string Position = ""; // position of trigger row 0/Else/1

        public BehaviourScope Scope = new BehaviourScope(); // Scope around trigger row

        public string MessageName = "";

        public EfeuMessageTag MessageTag;

        public int EffectId; // effect to resume
    }
}
