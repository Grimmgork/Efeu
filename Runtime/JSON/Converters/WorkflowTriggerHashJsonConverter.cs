using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Runtime.JSON.Converters
{
    public class WorkflowTriggerHashJsonConverter : JsonConverter<WorkflowTriggerHash>
    {
        public override WorkflowTriggerHash Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return WorkflowTriggerHash.FromString(reader.GetString() ?? "");
        }

        public override void Write(Utf8JsonWriter writer, WorkflowTriggerHash value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
