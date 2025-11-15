using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;

namespace Efeu.Runtime.Model
{
    public enum WorkflowActionNodeType
    {
        Start,
        Trigger,
        Call, // call method
        For,
        While,
        Repeat,
        If,
        Unless,
        Wait, // wait for trigger
        Join,
        Either
    }

    public class WorkflowActionNode
    {
        public int Id { get; set; }
        public WorkflowActionNodeType Type { get; set; }
        public string Name { get; set; } = "";
        public int XPos { get; set; }
        public int YPos { get; set; }
        public int Dispatch { get; set; }
        public int Then { get; set; }
        public int Error { get; set; }
        public string Script { get; set; } = "";
        public WorkflowInputNode Input { get; set; } = new WorkflowInputNode();
        public WorkflowOutputNode Output { get; set; } = new WorkflowOutputNode();
        public WorkflowOutputNode DispatchOutput { get; set; } = new WorkflowOutputNode();
        public Dictionary<string, int> Routes { get; set; } = new Dictionary<string, int>();
    }
}
