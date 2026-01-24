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
        private readonly IDeduplicationKeyRepository deduplicationKeyRepository;

        public DeduplicationKeyCommands(IDeduplicationKeyRepository deduplicationKeyRepository)
        {
            this.deduplicationKeyRepository = deduplicationKeyRepository;
        }

        public async Task<bool> TryInsertAsync(string key, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            await deduplicationKeyRepository.ClearBeforeAsync(timestamp - TimeSpan.FromDays(5));
            int result = await deduplicationKeyRepository.TryInsertAsync(key, timestamp);
            return result != 0;
        }

        public Task<bool> TryInsertAsync(Guid key, DateTimeOffset timestamp)
        {
            if (key == Guid.Empty)
                return Task.FromResult(true);

            return TryInsertAsync(key.ToString(), timestamp);
        }
    }
}
