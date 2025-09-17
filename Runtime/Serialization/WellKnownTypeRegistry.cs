using Efeu.Runtime.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public class WellKnownTypeRegistry : IWellKnownTypeRegistry
    {
        private readonly Dictionary<string, Type> nameToType = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> typeToName = new Dictionary<Type, string>();

        public void Register<T>(string name)
        {
            Type type = typeof(T);
            nameToType[name] = type;
            typeToName[type] = name;
        }

        public void Register<T>()
        {
            Register<T>(typeof(T).Name);
        }

        public Type GetTypeByName(string typeName)
        {
            return nameToType[typeName];
        }

        public string GetNameOfType(Type type)
        {
            return typeToName[type];
        }

        public static WellKnownTypeRegistry WithEfeuTypes()
        {
            WellKnownTypeRegistry typeRegistry = new WellKnownTypeRegistry();
            typeRegistry.Register<EfeuWrapper>();
            typeRegistry.Register<EfeuString>();
            typeRegistry.Register<EfeuArray>();
            typeRegistry.Register<EfeuHash>();
            typeRegistry.Register<EfeuDecimal>();
            typeRegistry.Register<EfeuFloat>();
            typeRegistry.Register<EfeuTime>();
            return typeRegistry;
        }
    }
}
