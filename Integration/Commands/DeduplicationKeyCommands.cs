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
        private readonly IEfeuUnitOfWork unitOfWork;

        public DeduplicationKeyCommands(IDeduplicationKeyRepository deduplicationKeyRepository, IEfeuUnitOfWork unitOfWork)
        {
            this.deduplicationKeyRepository = deduplicationKeyRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<bool> TryInsertAsync(string key, DateTimeOffset timestamp)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            await unitOfWork.BeginAsync();
            await deduplicationKeyRepository.ClearBeforeAsync(timestamp.Subtract(TimeSpan.FromDays(5)));
            int result = await deduplicationKeyRepository.TryInsertAsync(key, timestamp);
            await unitOfWork.CompleteAsync();
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
