using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Model
{
    public class WorkflowFunctionDescription
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Func<IWorkflowFunction>? Factory { get; set; }
    }
}
