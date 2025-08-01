using Efeu.Runtime;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Model
{
    public class WorkflowInstanceEntity
    {
        public int Id { get; set; }
        public int WorkflowDefinitionVersionId { get; set; }
        public WorkflowExecutionState ExecutionState { get; set; }
        public WorkflowInstanceState State { get; set; }
        public int CurrentMethodId { get; set; }
        public SomeData Input { get; set; } = new SomeData();
        public SomeData Output { get; set; } = new SomeData();
        public SomeStruct Variables { get; set; } = new SomeStruct();
        public IDictionary<int, SomeData> MethodData { get; set; } = new Dictionary<int, SomeData>();
        public IDictionary<int, SomeData> MethodOutput { get; set; } = new Dictionary<int, SomeData>();
        public SomeData DispatchResult { get; set; }
        public Stack<int> ReturnStack { get; set; } = new Stack<int>();
    }
}
