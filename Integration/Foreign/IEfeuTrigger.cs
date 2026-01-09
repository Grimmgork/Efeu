using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public interface IEfeuTrigger
    {
        public Task AttachAsync(EfeuTriggerContext context, CancellationToken token);

        public Task DetatchAsync(EfeuTriggerContext context);
    }
}
