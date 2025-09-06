using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Model
{
    public class WorkflowInputNode
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int ActionId { get; set; }
        public DataTraversal Traversal { get; set; }
        public SomeData Value { get; set; }
        public WorkflowInputType Type { get; set; }
        public string Name { get; set; } = "";
        public string Definition { get; set; } = "";
        public List<WorkflowInputNode> Inputs { get; set; } = [];
    }
}
