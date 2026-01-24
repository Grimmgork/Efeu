using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IDeduplicationKeyRepository
    {
        public Task<int> TryInsertAsync(string key, DateTimeOffset timestamp);

        public Task ClearBeforeAsync(DateTimeOffset timestamp);
    }
}
