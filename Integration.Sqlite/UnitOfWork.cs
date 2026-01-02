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
        private readonly DataConnection connection;
        private bool isTransactionRunning;
        private readonly Guid id;
        private Guid messageId;

        public UnitOfWork(DataConnection connection)
        {
            this.connection = connection;
            this.id = Guid.NewGuid();
        }

        public Task BeginAsync(Guid messageId)
        {
            this.messageId = messageId;
            return BeginAsync();
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

            try
            {
                await connection.GetTable<LockEntity>().DeleteAsync(i => i.Bundle == id);
                await connection.CommitTransactionAsync();
                isTransactionRunning = false;
            }
            catch (Exception ex)
            {
                isTransactionRunning = false;
                await connection.CloseAsync();
                throw new AggregateException("Commit failed, connection closed.", ex);
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (isTransactionRunning)
                    await connection.RollbackTransactionAsync();
            }
            catch (Exception ex)
            {
                await connection.CloseAsync();
                throw new AggregateException("Rollback failed, connection closed.", ex);
            }
        }

        public async Task LockAsync(params string[] locks)
        {
            if (!isTransactionRunning)
                throw new Exception("No transaction is running!");

            await connection.BulkCopyAsync(new BulkCopyOptions()
            {
                BulkCopyType = BulkCopyType.MultipleRows
            }, locks.Select(i => new LockEntity()
            {
                Name = i,
                Bundle = id
            }));
        }

        public async Task DoAsync(Func<Task> task)
        {
            try
            {
                await BeginAsync();
                await task();
                await CommitAsync();
            }
            catch (Exception ex)
            {
                await RollbackAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (isTransactionRunning)
                await connection.RollbackTransactionAsync();

            connection.Dispose();
        }
    }
}
