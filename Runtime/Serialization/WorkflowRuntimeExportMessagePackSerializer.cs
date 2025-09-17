using Efeu.Runtime.Data;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public class WorkflowRuntimeExportMessagePackSerializer : IWorkflowRuntimeExportSerializer
    {
        private readonly IMessagePackFormatter[] formatters = [
            new EfeuHashFormatter(),
            new EfeuArrayFormatter(),
            new WorkflowTriggerHashFormatter()
        ];

        private readonly IWellKnownTypeRegistry typeRegistry;

        public WorkflowRuntimeExportMessagePackSerializer(IWellKnownTypeRegistry typeRegistry, params IMessagePackFormatter[] formatters)
        {
            this.formatters = this.formatters.Concat(formatters).ToArray();
            this.typeRegistry = typeRegistry;
        }

        public WorkflowRuntimeExportMessagePackSerializer(params IMessagePackFormatter[] formatters)
        {
            this.formatters = this.formatters.Concat(formatters).ToArray();
            this.typeRegistry = WellKnownTypeRegistry.WithEfeuTypes();
        }

        private MessagePackSerializerOptions GetSerializerOptions()
        {
            return MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(formatters, [new ReferencePreservingEfeuValueResolver(typeRegistry), StandardResolver.Instance]));
        }

        public WorkflowRuntimeExport Deserialize(byte[] bytes)
        {
            MessagePackSerializerOptions options = GetSerializerOptions();
            return MessagePackSerializer.Deserialize<WorkflowRuntimeExport>(bytes, options);
        }

        public byte[] Serialize(WorkflowRuntimeExport export)
        {
            MessagePackSerializerOptions options = GetSerializerOptions();
            return MessagePackSerializer.Serialize(export, options);
        }
    }
}
