using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite.Migrations
{
    public class _0001_Initial : IEfeuMigration
    {
        private readonly DataConnection connection;

        public _0001_Initial(DataConnection connection)
        {
            this.connection = connection;
        }

        public Task Up()
        {
            Console.WriteLine($"Up {1}");
            return Task.CompletedTask;
        }

        public Task Down()
        {
            Console.WriteLine($"Down {1}");
            return Task.CompletedTask;
        }
    }
}
