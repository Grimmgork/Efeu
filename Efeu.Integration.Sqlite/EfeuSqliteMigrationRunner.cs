using Efeu.Integration.Interfaces;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.Mapping;
using System;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    public class EfeuSqliteMigrationRunner : IEfeuMigrationRunner
    {
        public Task Run(string connectionString, string version)
        {
            // check version from database
            // while version is smaller than version
            //     run next migration
            throw new Exception();
        }
    }
}
