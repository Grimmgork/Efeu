using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface ISomeDataSerializer
    {
        public string Serialize(SomeData data);

        public SomeData Deserialize(string payload);
    }
}
