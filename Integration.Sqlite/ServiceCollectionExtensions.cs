using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Runtime.Value;
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
using Efeu.Integration.Sqlite.Queries;
using Efeu.Runtime;
using Efeu.Runtime.Json.Converters;
using System.Data.SqlClient;
using System.Data.Common;
using LinqToDB.DataProvider.SQLite;
using System.Data.SQLite;

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

        private static MappingSchema ConfigureMapping(IServiceProvider services, string schema)
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
            jsonOptions.IncludeFields = true;
            jsonOptions.Converters.Add(new EfeuValueJsonConverter());
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.Converters.Add(new EfeuRuntimeScopeJsonConverter());

            var builder = new FluentMappingBuilder();
            builder.MappingSchema.SetConverter<EfeuValue, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<IDictionary<int, EfeuValue>, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<Stack<int>, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<BehaviourDefinitionStep[], DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<EfeuRuntimeScope, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<DateTimeOffset, DataParameter>(c => new DataParameter(null, c.ToUnixTimeMilliseconds(), DataType.Long));

            builder.MappingSchema.SetConverter<string, EfeuValue>(i => ConvertFromJson<EfeuValue>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, Stack<int>>(i => ConvertFromJson<Stack<int>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, IDictionary<int, EfeuValue>>(i => ConvertFromJson<IDictionary<int, EfeuValue>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, BehaviourDefinitionStep[]>(i => ConvertFromJson<BehaviourDefinitionStep[]>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, EfeuRuntimeScope>(i => ConvertFromJson<EfeuRuntimeScope>(i, jsonOptions));
            builder.MappingSchema.SetConverter<long, DateTimeOffset>(DateTimeOffset.FromUnixTimeMilliseconds);

            builder.MappingSchema.SetDataType(typeof(DateTimeOffset), DataType.Int64);

            builder.Entity<BehaviourDefinitionEntity>()
                .HasTableName("Definition")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                .Property(p => p.Name)
                .Property(p => p.Version);

            builder.Entity<BehaviourDefinitionVersionEntity>()
                .HasTableName("DefinitionVersion")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                .Property(p => p.Version)
                .Property(p => p.Steps);

            builder.Entity<TriggerEntity>()
                .HasTableName("Trigger")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(false)
                .Property(p => p.CorrelationId)
                .Property(p => p.DefinitionVersionId)
                .Property(p => p.Name)
                .Property(p => p.Tag)
                .Property(p => p.Position)
                .Property(p => p.Scope)
                .Property(p => p.Matter);

            builder.Entity<EfeuEffectEntity>()
                .HasTableName("Effect")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(false)
                .Property(p => p.CorrelationId)
                .Property(p => p.CreationTime)
                .Property(p => p.State)
                .Property(p => p.Times)
                .Property(p => p.Name)
                .Property(p => p.Input)
                .Property(p => p.Data)
                .Property(p => p.LockId)
                .Property(p => p.LockedUntil)
                .Property(p => p.Matter);

            builder.Entity<LockEntity>()
                .HasTableName("Lock")
                .HasSchemaName(schema)
                .Property(p => p.Name)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(false)
                .Property(p => p.Bundle);

            builder.Entity<DeduplicationKeyEntity>()
                .HasTableName("DeduplicationKey")
                .HasSchemaName(schema)
                .Property(p => p.Key)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(false)
                .Property(p => p.Timestamp);

            builder.Build();

            return builder.MappingSchema;
        }

        public static void AddEfeuSqlite(this IServiceCollection services, string schema, string connectionString)
        {
            services.AddScoped((serviceProvider) => {
                var options = new DataOptions()
                    .UseSQLite(connectionString)
                    .UseMappingSchema(ConfigureMapping(serviceProvider, schema));
                return new DataConnection(options);
            });

            services.AddEfeuSqliteServices();
        }

        public static void AddEfeuSqlite(this IServiceCollection services, string schema)
        {
            services.AddScoped((serviceProvider) => {
                var options = new DataOptions()
                    .UseDataProvider(SQLiteTools.GetDataProvider(ProviderName.SQLite))
                    .UseConnection(serviceProvider.GetRequiredService<SQLiteConnection>())
                    .UseMappingSchema(ConfigureMapping(serviceProvider, schema));
                return new DataConnection(options);
            });

            services.AddEfeuSqliteServices();
        }

        private static void AddEfeuSqliteServices(this IServiceCollection services)
        {
            services.AddScoped<EfeuUnitOfWork>();
            services.AddScoped<IEfeuUnitOfWork, EfeuUnitOfWork>();
            services.AddScoped<IBehaviourDefinitionQueries, BehaviourDefinitionQueries>();
            services.AddScoped<IEfeuTriggerQueries, BehaviourTriggerQueries>();
            services.AddScoped<IBehaviourEffectQueries, BehaviourEffectQueries>();
            services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
            services.AddScoped<IDeduplicationKeyQueries, DeduplicationKeyQueries>();
        }
    }
}
