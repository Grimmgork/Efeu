using Efeu.Integration.Persistence;
using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public interface IEfeuEngine
    {
        public IEfeuUnitOfWork UnitOfWork { get; }

        public Task ProcessSignalAsync(EfeuMessage message, Guid messageId, DateTimeOffset timestamp);

        public Task ClearDeduplicationKeysBeforeAsync(DateTimeOffset until);
    }
}
