using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Model
{
    public class WorkflowInputNode
    {
        public string Name { get; set; }
        public IInputSource Source { get; set; }

        public WorkflowInputNode(string name, IInputSource source)
        {
            Name = name;
            Source = source;
        }
    }
}
