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
        private readonly Guid id;

        private TransactionScope? scope;
        private DataConnection connection;

        private HashSet<string> locks = new HashSet<string>();

        public EfeuUnitOfWork(DataConnection connection)
        {
            this.connection = connection;
            this.id = Guid.NewGuid();
        }

        public Task BeginAsync()
        {
            if (scope != null)
                throw new InvalidOperationException("A transaction is already running.");

            scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            return Task.CompletedTask;
        }

        public async Task CompleteAsync()
        {
            if (scope == null)
                throw new InvalidOperationException("No transaction is running.");

            await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
            scope?.Complete();
            scope?.Dispose();
        }

        public async Task LockAsync(string key)
        {
            if (scope == null)
                throw new InvalidOperationException("No transaction is running.");

            if (locks.Contains(key))
                return;

            await connection.InsertAsync(new LockEntity()
            {
                Name = key,
                Bundle = id
            });

            locks.Add(key);
        }

        public void EnsureTransaction()
        {
            if (scope == null)
                throw new InvalidOperationException("No transaction is running.");
        }

        public async Task DoAsync(Func<Task> func)
        {
            await BeginAsync();
            await func();
            await CompleteAsync();
        }

        public ValueTask DisposeAsync()
        {
            Transaction.Current?.Rollback();
            scope?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
