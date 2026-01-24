using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IDeduplicationKeyCommands
    {
        public Task<bool> TryInsertAsync(string key, DateTimeOffset timestamp);

        public Task<bool> TryInsertAsync(Guid key, DateTimeOffset timestamp);
    }
}
