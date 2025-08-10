using Efeu.Integration.Data;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Efeu.Integration.Sqlite
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataConnection connection;

        public UnitOfWork(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task ExecuteAsync(Func<Task> action)
        {
            return ExecuteAsync(IsolationLevel.Unspecified, action);
        }

        public async Task ExecuteAsync(IsolationLevel isolationLevel, Func<Task> action)
        {
            await connection.BeginTransactionAsync(isolationLevel);
            try
            {
                await action();
            }
            catch (Exception)
            {
                await connection.RollbackTransactionAsync();
                throw;
            }

            await connection.CommitTransactionAsync();
        }
    }
}
