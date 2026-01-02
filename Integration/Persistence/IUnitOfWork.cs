using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        public Task DoAsync(Func<Task> task);

        public Task BeginAsync();

        public Task LockAsync(params string[] locks);

        public Task CommitAsync();

        public Task RollbackAsync();
    }
}
