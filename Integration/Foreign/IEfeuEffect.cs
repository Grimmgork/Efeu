using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public interface IEfeuEffect
    {
        public Task RunAsync(EfeuEffectExecutionContext context, CancellationToken token);
    }
}
