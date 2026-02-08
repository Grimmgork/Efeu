using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class EfeuTriggerMatch
    {
        public string Name = "";

        public EfeuBehaviourExpression Value = EfeuBehaviourExpression.Empty;

        public EfeuTriggerMatch[] Fields = [];
    }
    public class EfeuTrigger
    {
        public Guid Id;

        public Guid CorrelationId;

        public string Position = ""; // position of trigger row /0/Else/1

        public EfeuRuntimeScope Scope = new EfeuRuntimeScope(); // Scope around trigger row

        public string Type = "";

        public EfeuMessageTag Tag;

        public EfeuValue Input;

        public EfeuBehaviourStep Step = new EfeuBehaviourStep();

        public EfeuTriggerMatch[] Fields = [];

        public int BehaviourId;

        public Guid Matter;

        public bool IsStatic => CorrelationId == Guid.Empty; // a trigger is static if it is not assigned to a instance
    }
}
