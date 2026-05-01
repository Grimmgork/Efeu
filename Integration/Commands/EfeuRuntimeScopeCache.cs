using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class EfeuRuntimeScopeCache
    {
        private Dictionary<Guid, EfeuRuntimeScope> cache = new Dictionary<Guid, EfeuRuntimeScope>();

        public Task<EfeuRuntimeScope> GetAsync(Guid id)
        {
            return new EfeuRuntimeScope();
        }

        public void Add(EfeuRuntimeScope scope)
        {

        }

        public Task FlushAsync()
        {
            return Task.CompletedTask;
        }
    }
}
