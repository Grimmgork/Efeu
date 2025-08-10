using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Logic
{
    internal class WorkflowDefinitionEditor
    {
        private readonly WorkflowDefinition workflowDefinition;

        public WorkflowDefinitionEditor(WorkflowDefinition workflowDefinition)
        {
            this.workflowDefinition = workflowDefinition;

            foreach (WorkflowActionNode action in workflowDefinition.Actions)
            {
                actions.Add(action.Id, action);
                actionsIdCounter = action.Id;
            }

            // load into dictinaries
            // set counters
        }

        private readonly Dictionary<int, WorkflowActionNode> actions = new Dictionary<int, WorkflowActionNode>();

        private int actionsIdCounter;

        private int outputsIdCounter;

        private int inputsIdCounter;

        private readonly Dictionary<int, WorkflowOutputNode> outputs = new Dictionary<int, WorkflowOutputNode>();

        private readonly Dictionary<int, WorkflowInputNode> inputs = new Dictionary<int, WorkflowInputNode>();

        public IReadOnlyDictionary<int, WorkflowInputNode> InputNodes => inputs;

        public IReadOnlyDictionary<int, WorkflowOutputNode> OutputNodes => outputs;

        public IReadOnlyDictionary<int, WorkflowActionNode> ActionNodes => actions;

        public void AddActionNode(WorkflowActionNode node)
        {
            if (node.Id != 0)
                throw new InvalidOperationException("new nodes cannot have an id other than 0");
        }

        public void RemoveActionNode(int id)
        {

        }

        public void AddOutputNode(WorkflowOutputNode node)
        {

        }

        public void RemoveOutputNode(int actionId, DataTraversal traversal)
        {

        }

        public void AddInputNode(int actionId, DataTraversal traversal)
        {
            
        }

        public void RemoveInputNode(int actionId, DataTraversal traversal)
        {

        }
    }
}
