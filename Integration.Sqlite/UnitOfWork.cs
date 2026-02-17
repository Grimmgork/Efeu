using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Efeu.Integration.Sqlite
{
    internal class UnitOfWork : IEfeuUnitOfWork
    {
        private readonly Guid id;
        private readonly DataConnection connection;
        private readonly SQLiteConnection sqliteConnection;

        private TransactionScope? scope;


        private HashSet<string> locks = new HashSet<string>();
        private int depth;

        public UnitOfWork(DataConnection connection, SQLiteConnection sqliteConnection)
        {
            this.connection = connection;
            this.sqliteConnection = sqliteConnection;
            this.id = Guid.NewGuid();
        }

        public Task BeginAsync()
        {
            if (depth == 0)
            {
                TransactionOptions options = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.DefaultTimeout
                };

                scope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
            }
            depth++;
            return Task.CompletedTask;
        }

        public async Task CompleteAsync()
        {
            if (depth == 0)
                throw new InvalidOperationException("No transaction is running.");

            depth--;
            if (depth == 0)
            {
                try
                {
                    await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
                    scope?.Complete();
                }
                finally
                {
                    scope?.Dispose();
                }
            }
        }

        public async Task LockAsync(string key)
        {
            if (depth == 0)
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

        public Task ResetAsync()
        {
            scope?.Dispose();
            depth = 0;
            locks.Clear();
            return sqliteConnection.CloseAsync();
        }
    }
}
