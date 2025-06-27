using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Data;

namespace Workflows.Model
{
    public class WorkflowRouteNode
    {
        public string Name { get; set; } = "";
        public int ActionId { get; set; }
    }
}
