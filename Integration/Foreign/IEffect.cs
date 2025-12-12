using Efeu.Integration.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public enum EffectExecutionState
    {
        Completed,
        Suspended,
        Yield
    }

    public interface IEffect
    {
        public Task RunAsync(EffectExecutionContext context, CancellationToken token);
    }
}
