using Efeu.Integration.Persistence;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.Extensions;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    internal class MigrationRunner : IEfeuMigrationRunner
    {
        private readonly DataConnection connection;

        private readonly IEfeuMigration[] migrations;

        public MigrationRunner(DataConnection connection)
        {
            this.connection = connection;
            this.migrations = FindMigrationTypes().Select(i =>
                ((IEfeuMigration)Activator.CreateInstance(i, connection)!)).OrderBy(i => i.Version).ToArray();
        }

        public async Task MigrateToAsync(int desiredVersion)
        {
            if (migrations.Length == 0)
                return;

            // pick latest version
            if (desiredVersion == 0)
            {
                desiredVersion = migrations.Last().Version;
            }

            DataConnectionTransaction transaction = await connection.BeginTransactionAsync();
            await using (transaction)
            {
                int currentVersion = await GetAppliedVersion();
                if (currentVersion == 0)
                {
                    await Setup();
                    await migrations.First().Up();
                    await connection.ExecuteAsync("INSERT INTO __Migration(Version) VALUES (@version)", [new ("version", 1)]);
                    currentVersion = 1;
                }

                IEfeuMigration currentMigration = migrations.First(i => i.Version == currentVersion);
                int currentMigrationIndex = Array.IndexOf(migrations, currentMigration);

                while (currentVersion > desiredVersion)
                {
                    // down
                    await currentMigration.Down();
                    currentMigration = migrations[--currentMigrationIndex];
                    currentVersion = currentMigration.Version;
                }

                while (currentVersion < desiredVersion)
                {
                    // up
                    currentMigration = migrations[++currentMigrationIndex];
                    currentVersion = currentMigration.Version;
                    await currentMigration.Up();
                    await connection.ExecuteAsync("INSERT INTO __Migration(Version) VALUES (@version)", [new("version", currentVersion)]);
                }

                await transaction.CommitAsync();
            }
        }

        private IEnumerable<TypeInfo> FindMigrationTypes() => 
                    Assembly.GetAssembly(typeof(MigrationRunner))?.DefinedTypes.Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(IEfeuMigration).IsAssignableFrom(t)) ?? [];

        public async Task<int> GetAppliedVersion()
        {
            int exists = await connection.ExecuteAsync<int>("SELECT COUNT( ) FROM sqlite_master WHERE type='table' AND name='__Migration';");
            if (exists == 0)
                return 0;

            int version = await connection.ExecuteAsync<int>("SELECT Version FROM __Migration ORDER BY Version DESC LIMIT 1;");
            return version;
        }

        public Task Setup()
        {
            return connection.ExecuteAsync("CREATE TABLE IF NOT EXISTS __Migration (Version INTEGER);");
        }

        public int[] GetAllVersions()
        {
            return migrations.Select(i => i.Version).ToArray();
        }

        public Task<IEfeuMigration> GetMigrationInstance(int version)
        {
            return Task.FromResult(migrations.First(i => i.Version == version));
        }
    }
}
