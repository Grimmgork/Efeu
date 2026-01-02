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
    internal class WriteConsoleEffect : IEffect
    {
        public async Task RunAsync(EffectExecutionContext context, CancellationToken token)
        {
            Console.WriteLine(context.Input);
            await context.CompleteAsync();
        }
    }
}
