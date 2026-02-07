using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class TriggerEntity
    {
        public Guid Id;

        public DateTimeOffset CreationTime;

        public EfeuValue Input;

        public Guid CorrelationId;

        public int DefinitionVersionId;

        public string Position = ""; // position of trigger row 0/Else/1

        public EfeuRuntimeScope Scope = new EfeuRuntimeScope(); // Scope around trigger row

        public string Type = "";

        public EfeuMessageTag Tag;

        public Guid Matter;
    }
}
