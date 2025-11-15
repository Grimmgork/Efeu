using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public interface IWorkflowOutbox
    {
        public Task SendAsync(object message);

        public Task SendMultipleAsync(params object[] messages);
    }
}
