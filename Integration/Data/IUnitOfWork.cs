using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Data
{
    public interface IUnitOfWork
    {
        public Task ExecuteAsync(Func<Task> action);

        public Task ExecuteAsync(IsolationLevel isolationLevel, Func<Task> action);
    }
}
