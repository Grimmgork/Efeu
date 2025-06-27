using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workflows.Message;

namespace Workflows.Signal
{
    public class PromptInputSignal : WorkflowSignal
    {
        public readonly string Input;

        public PromptInputSignal(string input, DateTime time) : base(time)
        {
            Input = input;
        }
    }
}
