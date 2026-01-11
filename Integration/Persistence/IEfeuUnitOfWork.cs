using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Persistence
{
    public interface IEfeuUnitOfWork : IAsyncDisposable
    {
        public Task DoAsync(Func<Task> func);

        public Task BeginAsync();

        public Task CompleteAsync();

        public void EnsureTransaction();

        public Task LockAsync(string locks);
    }
}
