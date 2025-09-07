using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    // Type 2 1
    // Type 1
    // Type

    public struct WorkflowTriggerHash
    {
        public readonly string Type;
        public readonly string[] Arguments;

        public bool IsEmpty => string.IsNullOrEmpty(Type);

        public WorkflowTriggerHash(string type, params string[] args)
        {
            this.Type = type;
            this.Arguments = args;
        }

        public WorkflowTriggerHash[] Expand()
        {
            WorkflowTriggerHash[] result = new WorkflowTriggerHash[this.Arguments.Length + 1];
            result[0] = new WorkflowTriggerHash(Type);

            int i = 0;
            foreach (string arg in Arguments)
            {
                i++;
                result[i] = new WorkflowTriggerHash(Type, Arguments.Take(i).ToArray());
            }

            return result;
        }

        public override string? ToString()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Arguments?.Prepend(Type) ?? [""])));
        }

        public static WorkflowTriggerHash FromString(string hash)
        {
            string[] segments = JsonSerializer.Deserialize<string[]>(Convert.FromBase64String(hash)) ?? [];
            return new WorkflowTriggerHash(segments.FirstOrDefault() ?? "", segments?.Skip(1).ToArray() ?? []);
        }

        public bool Equals(WorkflowTriggerHash obj)
        {
            return ToString() == obj.ToString();
        }
    }
}
