using Efeu.Integration.Data;
using Efeu.Integration.Model;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.Data;
using Efeu.Runtime.Json;
using Efeu.Runtime.Model;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Integration.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEfeuSqlite(this IServiceCollection services, string connectionString, string schema = "efeu")
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new SomeDataJsonConverter());
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            MappingSchema mapping = new MappingSchema();
            mapping.SetConverter<SomeStruct, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            mapping.SetConverter<string, SomeStruct>(i => JsonSerializer.Deserialize<SomeStruct>(i, jsonOptions));
            mapping.SetConverter<SomeData, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            mapping.SetConverter<string, SomeData>(i => JsonSerializer.Deserialize<SomeData>(i, jsonOptions));
            mapping.SetConverter<Stack<int>, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            mapping.SetConverter<string, Stack<int>>(i => JsonSerializer.Deserialize<Stack<int>>(i, jsonOptions));
            mapping.SetConverter<IDictionary<int, SomeData>, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            mapping.SetConverter<string, IDictionary<int, SomeData>>(i => JsonSerializer.Deserialize<IDictionary<int, SomeData>>(i, jsonOptions));

            var builder = new FluentMappingBuilder(mapping);
            builder.Entity<WorkflowDefinitionEntity>()
                .HasTableName("WorkflowDefinition")
                .HasSchemaName(schema);

            builder.Entity<WorkflowDefinitionVersionEntity>()
                .HasTableName("WorkflowDefinitionVersion")
                .HasSchemaName(schema);

            builder.Entity<WorkflowInstanceEntity>()
                .HasTableName("WorkflowInstance")
                .HasSchemaName(schema)
                .Property(i => i.Input)
                .HasConversion(v => JsonSerializer.Serialize(v, jsonOptions), v => JsonSerializer.Deserialize<SomeData>(v, jsonOptions));

            builder.Build();

            var options = new DataOptions()
                .UseSQLite(connectionString)
                .UseMappingSchema(mapping);

            services.AddScoped(provider => new DataConnection(options));
            services.AddScoped<SqliteUnitOfWork>();
            services.AddScoped<IUnitOfWork, SqliteUnitOfWork>();
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
        }
    }
}
