using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Intrinsics.X86;
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
            context.Output = context.Input.AsHash()
                .With("name", 1)
                .With("name", 2);

            context.Output = EfeuHash.Empty
                .With("name", 2)
                .Remove("name");

            context.Output = EfeuArray.Empty
                .Push(1)
                .Pop()
                .Unshift(1, 2, 3)
                .Shift(2)
                .First();

            Console.WriteLine(context.Input);
            return Task.FromResult(EfeuEffectResult.Suspended);
        }
    }
}
