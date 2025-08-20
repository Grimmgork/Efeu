using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Trigger
{
    public class ConsoleInputSignal : IWorkflowSignal
    {
        public string Input = "";

        public string GetTriggerHash()
        {
            return nameof(ConsoleInputSignal);
        }
    }
}
