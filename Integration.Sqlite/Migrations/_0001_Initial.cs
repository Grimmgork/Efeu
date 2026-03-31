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
            await connection.ExecuteAsync("CREATE TABLE Behaviour (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Version INTEGER, UNIQUE(Name))");
            await connection.ExecuteAsync("CREATE TABLE BehaviourVersion (Id INTEGER PRIMARY KEY AUTOINCREMENT, BehaviourId INTEGER, Version INTEGER, Steps TEXT, FOREIGN KEY(BehaviourId) REFERENCES Behaviour(Id))");
            await connection.ExecuteAsync("CREATE TABLE Trigger (Id TEXT PRIMARY KEY, BehaviourVersionId INTEGER, CorrelationId TEXT, CreationTime INTEGER, Input TEXT, Position TEXT, ScopeId TEXT, [Group] TEXT, Type TEXT, Tag TEXT, Matter TEXT, IsCompleted INTEGER, FOREIGN KEY(BehaviourVersionId) REFERENCES BehaviourVersion(Id))");
            await connection.ExecuteAsync("CREATE TABLE Effect (Id TEXT PRIMARY KEY, Type TEXT, CorrelationId TEXT, Input TEXT, Data TEXT, CreationTime INTEGER, State TEXT, Times INTEGER, ExecutionTime INTEGER, Fault TEXT, Tag INTEGER, Matter TEXT, LockId TEXT, LockedUntil INTEGER)");
            await connection.ExecuteAsync("CREATE TABLE Lock (Name TEXT PRIMARY KEY, Bundle TEXT)");
            await connection.ExecuteAsync("CREATE TABLE DeduplicationKey (Key TEXT PRIMARY KEY, Timestamp INTEGER)");
            await connection.ExecuteAsync("CREATE TABLE BehaviourScope (Id TEXT PRIMARY KEY, ReferenceCount INTEGER, Constants TEXT)");
        }

        public async Task Down()
        {
            await connection.ExecuteAsync("DROP TABLE Trigger");
            await connection.ExecuteAsync("DROP TABLE Effect");
            await connection.ExecuteAsync("DROP TABLE Lock");
            await connection.ExecuteAsync("DROP TABLE BahaviourVersion");
            await connection.ExecuteAsync("DROP TABLE Behaviour");
            await connection.ExecuteAsync("DROP TABLE DeduplicationKey");
            await connection.ExecuteAsync("DROP TABLE BehaviourScope");
        }
    }
}
