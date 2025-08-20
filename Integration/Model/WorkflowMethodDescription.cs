using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Model
{
    public class WorkflowMethodDescription
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Func<IWorkflowMethod>? Factory { get; set; }
    }
}
