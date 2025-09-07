using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    internal class ConsoleWorkflowOutbox : IWorkflowOutbox
    {
        public Task SendAsync(object message)
        {
            throw new NotImplementedException();
        }

        public Task SendMultipleAsync(object message)
        {
            throw new NotImplementedException();
        }
    }
}
