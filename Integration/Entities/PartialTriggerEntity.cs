using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class PartialTriggerEntity
    {
        public Guid Id;

        public DateTimeOffset CreationTime;

        public string Type = "";

        public EfeuMessageTag Tag;

        public Guid Matter;

        public Guid Group;
    }
}
