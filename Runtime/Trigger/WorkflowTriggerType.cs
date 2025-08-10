using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    public enum WorkflowTriggerType
    {
        None,
        Signal,
        Start,
        CronJob,
        Sleep
    }
}
