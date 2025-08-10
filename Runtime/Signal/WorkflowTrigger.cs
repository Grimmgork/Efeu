using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    public struct WorkflowTrigger : IWorkflowTrigger
    {
        public readonly WorkflowTriggerType Type;

        private readonly string[] signalArguments = [];

        private readonly string signalName = "";

        public WorkflowTrigger(string name, params string[] arguments)
        {
            this.signalArguments = arguments;
            this.signalName = name;
        }

        public static WorkflowTrigger Signal(string name, params string[] arguments) => new WorkflowTrigger(name, arguments);

        public static WorkflowTrigger Start() => new WorkflowTrigger(); // TODO

        public static WorkflowTrigger Cron() => new WorkflowTrigger(); // TODO

        public static WorkflowTrigger Sleep() => new WorkflowTrigger(); // TODO

        public new string ToString()
        {
            if (string.IsNullOrEmpty(signalName))
            {
                return "";
            }
            else
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signalArguments.Prepend(signalName))));
            }
        }

        public WorkflowTrigger GetTrigger()
        {
            return this;
        }
    }
}
