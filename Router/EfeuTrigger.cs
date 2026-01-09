using Efeu.Router.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public class EfeuTrigger
    {
        public Guid Id;

        public Guid CorrelationId;

        public string Position = ""; // position of trigger row /0/Else/1

        public EfeuRuntimeScope Scope = new EfeuRuntimeScope(); // Scope around trigger row

        public string Name = "";

        public EfeuMessageTag Tag;

        public EfeuValue Input;

        public BehaviourDefinitionStep Step = new BehaviourDefinitionStep();

        public EfeuTriggerField[] Fields = [];

        public int DefinitionId;

        public bool IsStatic => CorrelationId == Guid.Empty; // a trigger is static if it is not assigned to a instance
    }

    public class EfeuTriggerField
    {
        public string Name = "";

        public EfeuValue Literal;

        public EfeuTriggerField[] Fields = [];
    }
}
