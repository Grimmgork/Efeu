using Efeu.Runtime.Data;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efeu.Runtime.MessagePack.Formatters
{
    public class EfeuArrayFormatter : IMessagePackFormatter<EfeuArray?>
    {
        public EfeuArray? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new EfeuArray(MessagePackSerializer.Deserialize<EfeuValue[]>(ref reader, options));
        }

        public void Serialize(ref MessagePackWriter writer, EfeuArray? value, MessagePackSerializerOptions options)
        {
            MessagePackSerializer.Serialize(ref writer, value?.Items, options);
        }
    }
}
