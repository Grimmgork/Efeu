using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Model
{
    public class ComputationDefinition
    {
        public string Name { get; set; }
        public ICollection<WorkflowInputNode> Inputs { get; set; }
    }
}
