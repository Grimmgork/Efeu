using Efeu.Integration.Model;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Entities
{
    public class WorkflowInstanceEntity
    {
        public int Id { get; set; }
        public int WorkflowDefinitionVersionId { get; set; }
        public WorkflowExecutionState ExecutionState { get; set; }
        public WorkflowRuntimeExport Export { get; set; } = new WorkflowRuntimeExport();
    }
}
