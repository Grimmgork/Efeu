using Efeu.Integration.Persistence;
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

        public async Task Up()
        {
            await connection.ExecuteAsync("CREATE TABLE Definition (Id INTEGER PRIMARY KEY, Name TEXT, Version INTEGER, Steps TEXT)");
            await connection.ExecuteAsync("CREATE TABLE Trigger (Id TEXT PRIMARY KEY, DefinitionId INTEGER, CorrelationId TEXT, Position TEXT, Scope TEXT, MessageName TEXT, MessageTag TEXT, FOREIGN KEY(DefinitionId) REFERENCES Definition(Id))");
            await connection.ExecuteAsync("CREATE TABLE Effect (Id INTEGER PRIMARY KEY, Name TEXT, CorrelationId TEXT, TriggerId TEXT, Data TEXT, CreationTime INTEGER, CompletionTime INTEGER, State TEXT)");
        }

        public async Task Down()
        {
            await connection.ExecuteAsync("DROP TABLE Definition");
            await connection.ExecuteAsync("DROP TABLE Trigger");
            await connection.ExecuteAsync("DROP TABLE Effect");
        }
    }
}
