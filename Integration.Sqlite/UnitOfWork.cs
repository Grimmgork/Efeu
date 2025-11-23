using Efeu.Integration.Persistence;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Efeu.Integration.Sqlite
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly SqliteDataConnection connection;

        public UnitOfWork(SqliteDataConnection connection)
        {
            this.connection = connection;
        }

        public Task ExecuteAsync(Func<Task> action) => 
            ExecuteAsync(IsolationLevel.Serializable, action);

        public async Task ExecuteAsync(IsolationLevel isolationLevel, Func<Task> action)
        {
            if (connection.Transaction != null)
                throw new InvalidOperationException("Another Transaction has already started!");

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
