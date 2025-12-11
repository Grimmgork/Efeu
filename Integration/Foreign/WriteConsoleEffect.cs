using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    internal class WriteConsoleEffect : IEffect
    {
        public Task RunAsync(EffectExecutionContext context, CancellationToken token)
        {
            Console.WriteLine("hello there!");
            return Task.CompletedTask;
        }
    }
}
