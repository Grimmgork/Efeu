using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite.Repositories
{
    internal class DeduplicationKeyRepository : IDeduplicationKeyRepository
    {
        private readonly DataConnection connection;

        public DeduplicationKeyRepository(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task ClearBeforeAsync(DateTimeOffset timestamp)
        {
            return connection.GetTable<DeduplicationKeyEntity>()
                .DeleteAsync(i => i.Timestamp < timestamp);
        }

        public async Task<int> TryInsertAsync(string key, DateTimeOffset timestamp)
        {
            try
            {
                await connection.InsertAsync(new DeduplicationKeyEntity()
                {
                    Key = key,
                    Timestamp = timestamp
                });
            }
            catch (Exception ex)
            {
                if (ex is SQLiteException sqliteEx &&
                    sqliteEx.ResultCode == SQLiteErrorCode.Constraint)
                    return 0;

                throw;
            }
            return 1;
        }
    }
}
