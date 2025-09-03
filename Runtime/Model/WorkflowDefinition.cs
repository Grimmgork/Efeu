using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Model
{
    /// <summary>
    /// TODO:
    /// Entry points can be one with type <see cref="WorkflowActionNodeType.Start"/>
    /// and all <see cref="WorkflowActionNodeType.Trigger"/> wich have no previous actions.
    /// </summary>
    public class WorkflowDefinition
    {
        // public int StartId { get; set; }

        public int[] EntryPoints { get; set; } = [];

        public List<WorkflowActionNode> Actions { get; set; } = [];

        public WorkflowActionNode GetAction(int id)
        {
            return Actions.First(i => i.Id == id);
        }
    }
}
