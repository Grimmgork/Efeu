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
        public SomeDataTraversal Traversal { get; set; }
        public SomeData Literal { get; set; }
        [JsonIgnore]
        public Func<InputEvaluationContext, SomeData> CSharp { get; set; } = (context) => default;
        public WorkflowInputType Type { get; set; }
        public string Name { get; set; } = "";
        public WorkflowInputNode[] Inputs { get; set; } = [];
    }
}
