using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public enum EfeuMessageTag
    {
        Effect, // effect
        Data,   // incoming data signal
        Result, // incoming effect result signal
        Fault,  // incoming effect fault signal
    }

    public class EfeuMessage
    {
        public Guid Id;

        public string Type = "";

        public DateTimeOffset Timestamp;

        public EfeuMessageTag Tag;

        public Guid CorrelationId; // from wich it came

        public EfeuValue Payload;

        public Guid Matter;
    }
}
