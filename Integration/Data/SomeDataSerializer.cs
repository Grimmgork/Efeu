using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    internal class SomeDataSerializer : ISomeDataSerializer
    {
        public SomeData Deserialize(string payload)
        {
            throw new NotImplementedException();
        }

        public string Serialize(SomeData data)
        {
            throw new NotImplementedException();
        }
    }
}
