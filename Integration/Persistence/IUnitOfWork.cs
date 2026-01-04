using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        public Task DoAsync(Func<Task> func);

        public Task DoAsync(DbTransaction transaction, Func<Task> func);

        public void EnsureTransaction();

        public Task LockAsync(params string[] locks);

        public DbConnection Connection { get; }
    }
}
