using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public class WorkflowOutputNode
    {
        public string Name { get; set; } = "";
        public int Index { get; set; }
        public string Variable { get; set; } = "";

        public WorkflowOutputType Type { get; set; }
        public List<WorkflowOutputNode> Outputs { get; set; } = [];
    }
}
