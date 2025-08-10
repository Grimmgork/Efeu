using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Model
{
    public class WorkflowExecutionResult
    {
        public int WorkflowInstanceId;

        public TimeSpan Duration;

        public WorkflowExecutionState ExecutionState;

        public SomeData Output;

        public Exception? Exception;
    }
}
