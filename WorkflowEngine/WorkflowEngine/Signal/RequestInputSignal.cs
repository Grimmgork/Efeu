using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Message;

namespace Workflows.Signal
{
    public class RequestInputSignal : WorkflowSignal
    {
        public RequestInputSignal(DateTime time) : base(time)
        {

        }
    }
}
