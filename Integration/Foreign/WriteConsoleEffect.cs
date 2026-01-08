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
        private readonly IEfeuEngine efeu;

        public WriteConsoleEffect(IEfeuEngine efeu)
        {
            this.efeu = efeu;
        }

        public Task RunAsync(EfeuEffectExecutionContext context, CancellationToken token)
        {
            Console.WriteLine(context.Input);
            return Task.CompletedTask;
        }
    }
}
