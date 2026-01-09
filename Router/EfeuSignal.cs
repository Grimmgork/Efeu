using Efeu.Router.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public class EfeuSignal
    {
        public Guid Id;

        public string Name = "";

        public EfeuValue Data;

        public DateTimeOffset Timestamp;

        public Guid TriggerId;

        public EfeuMessageTag Tag;
    }
}
