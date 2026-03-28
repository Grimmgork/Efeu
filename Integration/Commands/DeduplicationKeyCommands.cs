using Efeu.Integration.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class DeduplicationKeyCommands : IDeduplicationKeyCommands
    {
        private readonly IDeduplicationKeyQueries deduplicationKeyQueries;

        public DeduplicationKeyCommands(IDeduplicationKeyQueries deduplicationKeyQueries)
        {
            this.deduplicationKeyQueries = deduplicationKeyQueries;
        }

        public Task CleanupAsync(DateTimeOffset timestamp)
        {
            return deduplicationKeyQueries.ClearBeforeAsync(timestamp.Subtract(TimeSpan.FromHours(12)));
        }

        public async Task<bool> TryInsertAsync(string key, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            int result = await deduplicationKeyQueries.TryInsertAsync(key, timestamp);
            return result != 0;
        }
    }
}
