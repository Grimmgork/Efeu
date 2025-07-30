using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Interfaces
{
    public interface IEfeuMigrationRunner
    {
        public Task Run(string connectionString, string version);
    }

    public interface IEfeuSqlMigration
    {
        public Task Run(SqlConnection connection)
        {
            throw new Exception();
        }
    }
}
