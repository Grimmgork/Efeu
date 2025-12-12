using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
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
        private readonly Guid lockBundle;

        public UnitOfWork(SqliteDataConnection connection)
        {
            this.connection = connection;
            this.lockBundle = Guid.NewGuid();
        }

        public async Task BeginAsync()
        {
            if (isTransactionRunning)
                throw new Exception("A transaction is already running!");

            isTransactionRunning = true;
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        public async Task CommitAsync()
        {
            if (!isTransactionRunning)
                throw new Exception("No transaction is running!");

            await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == lockBundle);
            await connection.CommitTransactionAsync();
            isTransactionRunning = false;
        }

        public Task RollbackAsync()
        {
            if (isTransactionRunning)
                return connection.RollbackTransactionAsync();
            else
                return Task.CompletedTask;
        }

        public async Task TryLockAsync(params string[] locks)
        {
            if (!isTransactionRunning)
                throw new Exception("No transaction is running!");

            await connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, locks.Select(i => new LockEntity()
            {
                Name = i,
                Bundle = lockBundle
            }));
        }

        public async ValueTask DisposeAsync()
        {
            if (isTransactionRunning)
                await connection.RollbackTransactionAsync();

            connection.Dispose();
        }
    }
}
