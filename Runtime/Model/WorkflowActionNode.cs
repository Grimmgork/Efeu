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
        Method,
        Function
    }

    public class WorkflowActionNode
    {
        public WorkflowActionNodeType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int ErrorRoute { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }
        public int DoReference { get; set; }
        public SomeDataTraversal DoOutputTraversal { get; set; }
        public int DefaultRoute { get; set; }
        public WorkflowInputNode Input { get; set; } = new WorkflowInputNode();
        public List<WorkflowRouteNode> Routes { get; set; } = [];
    }
}
