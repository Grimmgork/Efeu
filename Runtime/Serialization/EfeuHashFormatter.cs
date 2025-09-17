using Efeu.Runtime.Data;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public class EfeuHashFormatter : IMessagePackFormatter<EfeuHash?>
    {
        public EfeuHash Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new EfeuHash(MessagePackSerializer.Deserialize<IDictionary<string, EfeuValue>>(ref reader, options));
        }

        public void Serialize(ref MessagePackWriter writer, EfeuHash? value, MessagePackSerializerOptions options)
        {
            MessagePackSerializer.Serialize(ref writer, value?.Hash, options);
        }
    }
}
