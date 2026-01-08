using Efeu.Integration.Persistence;
using Efeu.Router;
using Efeu.Router.Value;
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

        public Task ProcessSignalAsync(EfeuSignal signal);

        public Task ClearDeduplicationKeysBeforeAsync(DateTimeOffset until);
    }
}
