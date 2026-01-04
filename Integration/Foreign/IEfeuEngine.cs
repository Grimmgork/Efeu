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
        public IUnitOfWork UnitOfWork { get; }

        public Task SendSignalAsync(EfeuMessage message, DateTimeOffset timestamp, string deduplication);
    }
}
