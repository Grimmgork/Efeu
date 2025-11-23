using Efeu.Integration.Entities;
using Efeu.Router;
using Efeu.Runtime.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public interface IEffect
    {
        public Task RunAsync(EffectExecutionContext context, CancellationToken token);
    }
}
