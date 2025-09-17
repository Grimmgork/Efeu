using Efeu.Runtime.Data;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public class ReferencePreservingEfeuValueResolver : IFormatterResolver
    {
        private readonly ReferencePreservingEfeuValueFormatter efeuValueFormatter;

        public ReferencePreservingEfeuValueResolver(IWellKnownTypeRegistry typeRegistry)
        {
            efeuValueFormatter = new ReferencePreservingEfeuValueFormatter(typeRegistry);
        }

        public IMessagePackFormatter<T>? GetFormatter<T>()
        {
            if (typeof(T) == typeof(EfeuValue))
            {
                return (IMessagePackFormatter<T>)efeuValueFormatter;
            }
            else
            {
                return null;
            }
        }
    }
}
