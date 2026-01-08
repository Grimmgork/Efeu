using Efeu.Router;
using Efeu.Router.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public class EfeuMessageProcessingData
    {
        public string Id = "";

        public string Name = "";

        public EfeuMessageTag Tag;

        public EfeuValue Data;

        public Guid CorrelationId;

        public Guid TriggerId;
    }
}
