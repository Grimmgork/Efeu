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

namespace Efeu.Integration.Sqlite
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly DataConnection connection;
        private readonly Guid id;

        private bool hasForeignTransaction;
        private bool broken;

        public DbConnection Connection => connection.Connection;

        public UnitOfWork(DataConnection connection)
        {
            this.connection = connection;
            this.id = Guid.NewGuid();
        }

        private Task BeginAsync()
        {
            return connection.BeginTransactionAsync();
        }

        private async Task CommitAsync(DbTransaction transaction)
        {
            if (connection.Transaction == null)
                throw new InvalidOperationException("No transaction is running.");

            try
            {
                await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                broken = true;
                throw new AggregateException("Commit failed.", ex);
            }
        }

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

        public ValueTask DisposeAsync()
        {
            return connection.DisposeAsync();
        }

        public void EnsureTransaction()
        {
            if (connection.Transaction == null && !hasForeignTransaction)
                throw new InvalidOperationException("Operation must be run in a transaction.");
        }

        public async Task DoAsync(Func<Task> func)
        {
            if (broken)
                throw new Exception("cannot reuse a broken uow.");

            try
            {
                await BeginAsync();
                await func();
                await CommitAsync(connection.Transaction!);
            }
            catch (Exception)
            {
                await connection.RollbackTransactionAsync();
                broken = true;
                throw;
            }
        }

        public async Task DoAsync(DbTransaction transaction, Func<Task> func)
        {
            if (broken)
                throw new Exception("cannot reuse a broken uow.");

            if (transaction.Connection != connection.Connection)
                throw new Exception("transaction is from another connection.");

            if (connection.Transaction != null && connection.Transaction != transaction)
                throw new Exception("another transaction is already running.");


            hasForeignTransaction = true;
            try
            {
                await func();
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
