﻿using Efeu.Integration.Data;
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
        private static T ConvertFromJson<T>(string json, JsonSerializerOptions jsonOptions)
        {
            T value = JsonSerializer.Deserialize<T>(json, jsonOptions)!;
            return value;
        }

        private static DataParameter ConvertToJson<T>(T value, JsonSerializerOptions jsonOptions)
        {
            string json = JsonSerializer.Serialize(value, jsonOptions);
            return new DataParameter(null, json, DataType.Text);
        }

        public static void AddEfeuSqlite(this IServiceCollection services, string connectionString, string schema = "efeu")
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new SomeDataJsonConverter());
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var builder = new FluentMappingBuilder();
            builder.MappingSchema.SetConverter<SomeData, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<SomeStruct, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<IDictionary<int, SomeData>, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<Stack<int>, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<WorkflowDefinition, DataParameter>(c => ConvertToJson(c, jsonOptions));
            
            builder.MappingSchema.SetConverter<string, SomeStruct>(i => ConvertFromJson<SomeStruct>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, SomeData>(i => ConvertFromJson<SomeData>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, Stack<int>>(i => ConvertFromJson<Stack<int>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, IDictionary<int, SomeData>>(i => ConvertFromJson<IDictionary<int, SomeData>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, WorkflowDefinition>(i => ConvertFromJson<WorkflowDefinition>(i, jsonOptions));

            builder.Entity<WorkflowDefinitionEntity>()
                .HasTableName("WorkflowDefinition")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(true)
                .Property(p => p.Definition);

            builder.Entity<WorkflowDefinitionVersionEntity>()
                .HasTableName("WorkflowDefinitionVersion")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(true)
                .Property(p => p.Definition);

            builder.Entity<WorkflowInstanceEntity>()
                .HasTableName("WorkflowInstance")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(true)
                .Property(p => p.Variables)
                .Property(p => p.ReturnStack)
                .Property(p => p.MethodOutput)
                .Property(p => p.MethodData);

            builder.Build();

            var options = new DataOptions()
                .UseSQLite(connectionString)
                .UseMappingSchema(builder.MappingSchema);

            services.AddScoped(provider => new DataConnection(options));
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IWorkflowDefinitionVersionRepository, WorkflowDefinitionVersionRepository>();
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
        }
    }
}
