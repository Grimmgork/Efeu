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
    internal class DeduplicationStore : IDeduplicationStore
    {
        private readonly DataConnection connection;

        public DeduplicationStore(DataConnection connection)
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
            if (string.IsNullOrWhiteSpace(key))
                return 1;

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
