using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Model
{
    public enum WorkflowExecutionState
    {
        Running,
        Suspended,
        Paused,
        Completed,
        Failed,
        Aborted
    }
}
