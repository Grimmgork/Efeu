using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class WorkflowInput : IInputSource
    {
        public SomeDataTraversal Name { get; set; } = "";

        public SomeData GetValue(InputEvaluationContext context)
        {
            return context.WorkflowInput.Traverse(Name);
        }
    }
}
