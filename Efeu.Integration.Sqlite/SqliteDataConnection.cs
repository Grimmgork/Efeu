using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class SqliteDataConnection : DataConnection
    {
        public SqliteDataConnection(DataOptions dataOptions): base(dataOptions)
        {

        }
    }
}
