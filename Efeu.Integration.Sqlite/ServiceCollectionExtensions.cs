using Efeu.Integration.Interfaces;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.Model;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEfeuSqlite(this IServiceCollection services, string connectionString, string schema = "efeu")
        {
            MappingSchema mapping = new MappingSchema();
            var builder = new FluentMappingBuilder(mapping);
            builder.Entity<WorkflowDefinitionEntity>()
                .HasTableName("WorkflowDefinition")
                .HasSchemaName(schema);

            builder.Build();

            var options = new DataOptions()
                .UseSQLite(connectionString)
                .UseMappingSchema(mapping);

            services.AddScoped(provider => new DataConnection(options));
            services.AddScoped<SqliteUnitOfWork>();
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
        }
    }
}
