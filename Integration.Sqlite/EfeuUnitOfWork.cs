using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Efeu.Integration.Sqlite
{
    internal class EfeuUnitOfWork : IEfeuUnitOfWork
    {
        private readonly DataConnection connection;
        private readonly Guid id;

        private bool hasForeignTransaction;
        private bool broken;

        public EfeuUnitOfWork(DataConnection connection)
        {
            this.connection = connection;
            this.id = Guid.NewGuid();
        }

        private Task BeginAsync()
        {
            return connection.BeginTransactionAsync();
        }

        private async Task CommitAsync()
        {
            if (connection.Transaction == null)
                throw new InvalidOperationException("No transaction is running.");

            await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
            await connection.Transaction.CommitAsync();
        }

        private async Task RollbackAsync()
        {
            if (connection.Transaction == null)
                throw new InvalidOperationException("No transaction is running.");

            await connection.Transaction.RollbackAsync();
        }

        public DbConnection GetConnection() => 
            connection.Connection;

        public async Task LockAsync(params string[] locks)
        {
            await connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, locks.Select(i => new LockEntity()
            {
                Name = i,
                Bundle = id
            }));
        }

        public void EnsureTransaction()
        {
            if (connection.Transaction == null && !hasForeignTransaction)
                throw new InvalidOperationException("Operation must be run in a transaction.");
        }

        public Task DoAsync(Func<Task> func)
        {
            return DoAsync((transaction) => func());
        }

        public async Task DoAsync(Func<DbTransaction, Task> func)
        {
            if (broken)
                throw new Exception("cannot reuse a broken uow.");

            try
            {
                await BeginAsync();
                await func(connection.Transaction!);
                await CommitAsync();
            }
            catch (Exception)
            {
                await RollbackAsync();
                broken = true;
                throw;
            }
        }

        public async Task DoAsync(DbTransaction transaction, Func<Task> func)
        {
            if (transaction.Connection != connection.Connection)
                throw new Exception("transaction is from another connection.");

            if (connection.Transaction != null && connection.Transaction != transaction)
                throw new Exception("another transaction is already running.");

            if (broken)
                throw new Exception("cannot reuse a broken uow.");

            hasForeignTransaction = true;
            try
            {
                await func();
                await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
                hasForeignTransaction = false;
            }
            catch (Exception)
            {
                hasForeignTransaction = false;
                broken = true;
                throw;
            }
        }
    }
}
