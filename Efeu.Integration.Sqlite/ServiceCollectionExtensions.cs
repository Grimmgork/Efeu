using Efeu.Integration.Data;
using Efeu.Integration.Model;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.Data;
using Efeu.Runtime.Json;
using Efeu.Runtime.Model;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
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

            var builder = new FluentMappingBuilder();
            //builder.MappingSchema.SetDataType(typeof(SomeStruct), new SqlDataType(DataType.Text, typeof(string)));
            //builder.MappingSchema.SetDataType(typeof(SomeData), new SqlDataType(DataType.Text, typeof(string)));
            builder.MappingSchema.SetConverter<SomeData, DataParameter>(c => new DataParameter(null, JsonSerializer.Serialize(c, jsonOptions)));
            builder.MappingSchema.SetConverter<SomeStruct, DataParameter>(c => new DataParameter(null, c)); // TODO add more like these

            builder.MappingSchema.SetConverter<SomeStruct, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, SomeStruct>(i => JsonSerializer.Deserialize<SomeStruct>(i, jsonOptions));
            builder.MappingSchema.SetConverter<SomeData, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, SomeData>(i => JsonSerializer.Deserialize<SomeData>(i, jsonOptions));
            builder.MappingSchema.SetConverter<Stack<int>, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, Stack<int>>(i => JsonSerializer.Deserialize<Stack<int>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<IDictionary<int, SomeData>, string>(i => JsonSerializer.Serialize(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, IDictionary<int, SomeData>>(i => JsonSerializer.Deserialize<IDictionary<int, SomeData>>(i, jsonOptions));

            builder.Entity<WorkflowDefinitionEntity>()
                .HasTableName("WorkflowDefinition")
                .HasSchemaName(schema);

            builder.Entity<WorkflowDefinitionVersionEntity>()
                .HasTableName("WorkflowDefinitionVersion")
                .HasSchemaName(schema);

            builder.Entity<WorkflowInstanceEntity>()
                .HasTableName("WorkflowInstance")
                .HasSchemaName(schema);
                // .HasConversion(v => JsonSerializer.Serialize(v, jsonOptions), v => JsonSerializer.Deserialize<SomeData>(v, jsonOptions));

            builder.Build();

            var options = new DataOptions()
                .UseSQLite(connectionString)
                .UseMappingSchema(builder.MappingSchema);

            services.AddScoped(provider => new DataConnection(options));
            services.AddScoped<SqliteUnitOfWork>();
            services.AddScoped<IUnitOfWork, SqliteUnitOfWork>();
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
        }
    }
}
