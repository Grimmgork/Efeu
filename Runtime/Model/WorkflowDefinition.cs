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

        public List<WorkflowActionNode> ActionNodes { get; set; } = [];

        public WorkflowDefinition(string name, int entryPointId)
        {
            Name = name;
            EntryPointId = entryPointId;
        }

        public WorkflowActionNode GetAction(int id)
        {
            return ActionNodes.First(i => i.Id == id);
        }

        public WorkflowActionNode Function(int id, string name)
        {
            WorkflowActionNode node = new WorkflowActionNode()
            {
                Type = WorkflowActionNodeType.Function,
                Id = id,
                Name = name
            };
            ActionNodes.Add(node);
            return node;
        }

        public WorkflowActionNode Method(int id, string name)
        {
            WorkflowActionNode node = new WorkflowActionNode()
            {
                Type = WorkflowActionNodeType.Method,
                Id = id,
                Name = name
            };
            ActionNodes.Add(node);
            return node;
        }
    }
}
