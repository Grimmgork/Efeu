using Efeu.Integration.Persistence;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly SqliteDataConnection connection;
        private bool isTransactionRunning;

        public UnitOfWork(SqliteDataConnection connection)
        {
            this.connection = connection;
        }

        public async Task BeginAsync()
        {
            if (isTransactionRunning)
                throw new Exception("A transaction is already running!");

            isTransactionRunning = true;
            await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        }

        public async Task CommitAsync()
        {
            if (!isTransactionRunning)
                throw new Exception("No transaction is running!");

            await connection.CommitTransactionAsync();
            isTransactionRunning = false;
        }

        public async ValueTask DisposeAsync()
        {
            if (isTransactionRunning)
                await connection.RollbackTransactionAsync();

            connection.Dispose();
        }
    }
}
