using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Model
{
    public class WorkflowDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public int EntryPointId { get; set; }

        public List<WorkflowActionNode> Actions { get; set; } = [];

        public WorkflowActionNode GetAction(int id)
        {
            return Actions.First(i => i.Id == id);
        }
    }
}
