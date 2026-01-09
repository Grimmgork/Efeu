using Efeu.Router.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public enum EfeuMessageTag
    {
        Outbox,
        Signal,
        Completion,
        Fault
    }

    public class EfeuMessage
    {
        public string Name = "";

        public EfeuMessageTag Tag;

        public Guid CorrelationId; // from wich it came

        public Guid TriggerId; // response / fault triggers id

        public EfeuValue Data;
    }
}
