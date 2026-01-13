using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Efeu.Integration.Foreign
{
    internal class WriteConsoleEffect : IEfeuEffect
    {
        public Task RunAsync(EfeuEffectExecutionContext context, CancellationToken token)
        {
            Console.WriteLine(context.Input);
            return context.SuspendAsync(new EfeuEffectTrigger()
            {
                Name = "Asdf",
                Tag = EfeuMessageTag.Data,
            });
        }

        public Task OnTriggerAsync(EfeuEffectTriggerContext context)
        {
            return context.SuspendAsync(default);
        }
    }
}
