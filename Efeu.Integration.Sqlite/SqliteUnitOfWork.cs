using Efeu.Integration.Interfaces;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    public class SqliteUnitOfWork : IUnitOfWork
    {
        public SqliteUnitOfWork(DataConnection connection)
        {

        }

        public Task ExecuteAsync(Func<Task> action)
        {
            throw new NotImplementedException();
        }
    }
}
