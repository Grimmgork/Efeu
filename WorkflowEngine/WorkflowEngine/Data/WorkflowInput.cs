using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflows.Data
{
    public class WorkflowInput : IInputSource
    {
        public SomeDataTraversal Name { get; set; } = "";

        public WorkflowInput(SomeDataTraversal traversal)
        {
            Name = traversal;
        }

        public SomeData GetValue(InputEvaluationContext context)
        {
            return context.WorkflowInput.Traverse(Name);
        }
    }
}
