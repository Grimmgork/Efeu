using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Efeu.Runtime.Signal
{
    public struct WorkflowSignalHash
    {
        private readonly string[] arguments = [];

        private readonly string name = "";

        public WorkflowSignalHash(string name, params string[] arguments)
        {
            this.arguments = arguments;
            this.name = name;
        }

        public static WorkflowSignalHash From(string name, params string[] arguments) => new WorkflowSignalHash(name, arguments);

        public new string ToString()
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            else
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(arguments.Prepend(name))));
            }
        }
    }
}
