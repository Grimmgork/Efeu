using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Interfaces
{
    public interface IUnitOfWork
    {
        public Task ExecuteAsync(Func<Task> action);
    }
}
