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
            await connection.ExecuteAsync("CREATE TABLE WorkflowDefinition (Id INTEGER PRIMARY KEY, Name TEXT)");
            await connection.ExecuteAsync("CREATE TABLE WorkflowDefinitionVersion (Id INTEGER PRIMARY KEY, Name TEXT, Definition TEXT, WorkflowDefinitionId INTEGER, FOREIGN KEY(WorkflowDefinitionId) REFERENCES WorkflowDefinition(Id))");
            await connection.ExecuteAsync("CREATE TABLE WorkflowInstance (Id INTEGER PRIMARY KEY, IsProcessing INTEGER, WorkflowDefinitionVersionId INTEGER, ExecutionState Integer, State INTEGER, CurrentMethodId INTEGER, Input TEXT, Output TEXT, MethodData TEXT, MethodOutput TEXT, DispatchResult TEXT, ReturnStack TEXT)");
        }

        public async Task Down()
        {
            await connection.ExecuteAsync("DROP TABLE WorkflowDefinition");
            await connection.ExecuteAsync("DROP TABLE WorkflowDefinitionVersion");
            await connection.ExecuteAsync("DROP TABLE WorkflowInstance");
        }
    }
}
