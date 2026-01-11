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
        public Task<EfeuEffectResult> RunAsync(EfeuEffectExecutionContext context, CancellationToken token)
        {
            Console.WriteLine(context.Input);
            return Task.FromResult(EfeuEffectResult.Completed);
        }
    }
}
