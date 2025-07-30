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
        private readonly DataConnection connection;

        public SqliteUnitOfWork(DataConnection connection)
        {
            this.connection = connection;
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            await connection.BeginTransactionAsync();
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
