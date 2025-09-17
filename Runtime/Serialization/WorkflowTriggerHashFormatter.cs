using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    internal class WorkflowTriggerHashFormatter : IMessagePackFormatter<WorkflowTriggerHash>
    {
        public WorkflowTriggerHash Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            string? value = reader.ReadString();
            return WorkflowTriggerHash.FromString(value ?? "");
        }

        public void Serialize(ref MessagePackWriter writer, WorkflowTriggerHash value, MessagePackSerializerOptions options)
        {
            writer.WriteString(Encoding.UTF8.GetBytes(value.ToString() ?? ""));
        }
    }
}
