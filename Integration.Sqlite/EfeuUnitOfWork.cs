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
        private int depth;

        public EfeuUnitOfWork(DataConnection connection)
        {
            this.connection = connection;
            this.id = Guid.NewGuid();
        }

        public Task BeginAsync()
        {
            if (depth == 0)
            {
                scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            }
            depth++;
            return Task.CompletedTask;
        }

        public async Task CompleteAsync()
        {
            if (depth == 1 && scope != null)
            {
                await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
                scope.Complete();
            }
            else
            {
                depth--;
            }
        }

        public async Task LockAsync(string key)
        {
            if (depth <= 0)
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

        public ValueTask DisposeAsync()
        {
            scope?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
