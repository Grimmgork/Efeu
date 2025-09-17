using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public interface IWellKnownTypeRegistry
    {
        public Type GetTypeByName(string typeName);

        public string GetNameOfType(Type type);
    }
}
