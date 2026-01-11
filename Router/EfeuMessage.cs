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
        Effect, // effect
        Data,   // incoming data signal
        Result, // incoming effect result signal
        Fault   // incoming effect fault signal
    }

    public class EfeuMessage
    {
        public Guid Id;

        public string Name = "";

        public DateTimeOffset Timestamp;

        public EfeuMessageTag Tag;

        public Guid CorrelationId; // from wich it came

        public Guid TriggerId; // response / fault triggers id

        public EfeuValue Data;
    }
}
