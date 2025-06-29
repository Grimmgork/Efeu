using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;

namespace Efeu.Runtime.Model
{
    public enum WorkflowActionNodeType
    {
        Method,
        Function
    }

    public class WorkflowActionNode
    {
        public WorkflowActionNodeType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int OnError { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }

        public int LambdaId { get; set; }

        [JsonIgnore]
        public Func<SomeData, SomeData> Lambda { get; set; }

        public int DefaultRoute { get; set; }
        public List<WorkflowInputNode> Inputs { get; set; } = [];
        public List<WorkflowRouteNode> Routes { get; set; } = [];

        public WorkflowActionNode Input(string name, IInputSource source)
        {
            Inputs.Add(new WorkflowInputNode(name, source)
            {
                Name = name,
                Source = source
            });
            return this;
        }

        public WorkflowActionNode Error(int methodId)
        {
            OnError = methodId;
            return this;
        }

        public WorkflowActionNode Then(int methodId)
        {
            DefaultRoute = methodId;
            return this;
        }

        public WorkflowActionNode When(string route, int methodId)
        {
            Routes.Add(new WorkflowRouteNode()
            {
                ActionId = methodId,
                Name = route
            });
            return this;
        }

        public WorkflowActionNode Do(int id)
        {
            LambdaId = id;
            return this;
        }

        public WorkflowActionNode Do(Func<SomeData, SomeData> lambda)
        {
            Lambda = lambda;
            return this;
        }
    }
}
