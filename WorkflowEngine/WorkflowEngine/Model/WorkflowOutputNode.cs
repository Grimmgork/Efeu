using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflows.Model
{
    public class WorkflowOutputNode
    {
        public readonly string Name;
        public readonly WorkflowActionNode Action;

        public WorkflowOutputNode(WorkflowActionNode function, string name)
        {
            Name = name;
            Action = function;
        }
    }
}
