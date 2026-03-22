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

        public async Task<bool> TryInsertAsync(string key, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            await deduplicationKeyQueries.ClearBeforeAsync(timestamp.Subtract(TimeSpan.FromDays(1)));
            int result = await deduplicationKeyQueries.TryInsertAsync(key, timestamp);
            return result != 0;
        }
    }
}
