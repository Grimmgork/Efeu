using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Router.Data;
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
using Efeu.Integration.Sqlite.Repositories;
using Efeu.Router;
using Efeu.Router.Json.Converters;

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

        private static SqliteDataConnection ConfigureConnection(IServiceProvider services, string connectionString, string schema)
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
            jsonOptions.IncludeFields = true;
            jsonOptions.Converters.Add(new EfeuValueJsonConverter());
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            var builder = new FluentMappingBuilder();
            builder.MappingSchema.SetConverter<EfeuValue, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<IDictionary<int, EfeuValue>, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<Stack<int>, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<BehaviourDefinitionStep[], DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<BehaviourScope, DataParameter>(c => ConvertToJson(c, jsonOptions));
            builder.MappingSchema.SetConverter<DateTimeOffset, DataParameter>(c => new DataParameter(null, c.ToUnixTimeMilliseconds(), DataType.Long));

            builder.MappingSchema.SetConverter<string, EfeuValue>(i => ConvertFromJson<EfeuValue>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, Stack<int>>(i => ConvertFromJson<Stack<int>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, IDictionary<int, EfeuValue>>(i => ConvertFromJson<IDictionary<int, EfeuValue>>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, BehaviourDefinitionStep[]>(i => ConvertFromJson<BehaviourDefinitionStep[]>(i, jsonOptions));
            builder.MappingSchema.SetConverter<string, BehaviourScope>(i => ConvertFromJson<BehaviourScope>(i, jsonOptions));
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

            builder.Entity<BehaviourTriggerEntity>()
                .HasTableName("Trigger")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                    .HasSkipOnInsert(false)
                .Property(p => p.CorrelationId)
                .Property(p => p.DefinitionVersionId)
                .Property(p => p.MessageName)
                .Property(p => p.MessageTag)
                .Property(p => p.Position)
                .Property(p => p.Scope)
                .Property(p => p.EffectId);

            builder.Entity<BehaviourEffectEntity>()
                .HasTableName("Effect")
                .HasSchemaName(schema)
                .Property(p => p.Id)
                    .IsIdentity()
                    .IsPrimaryKey()
                .Property(p => p.CorrelationId)
                .Property(p => p.CreationTime)
                .Property(p => p.State)
                .Property(p => p.Times)
                .Property(p => p.TriggerId)
                .Property(p => p.Name)
                .Property(p => p.Input)
                .Property(p => p.Data)
                .Property(p => p.LockId)
                .Property(p => p.LockedUntil);

            builder.Entity<LockEntity>()
                .HasTableName("Lock")
                .HasSchemaName(schema)
                .Property(p => p.Name)
                    .IsIdentity()
                    .IsPrimaryKey()
                .Property(p => p.Bundle);

            builder.Build();

            var options = new DataOptions()
                .UseSQLite(connectionString)
                .UseMappingSchema(builder.MappingSchema);

            return new SqliteDataConnection(options);
        }

        public static void AddEfeuSqlite(this IServiceCollection services, string connectionString, string schema = "efeu")
        {
            services.AddScoped((servicesProvider) => 
                ConfigureConnection(servicesProvider, connectionString, schema));

            services.AddScoped((servicesProvider) =>
                (DataConnection)ConfigureConnection(servicesProvider, connectionString, schema));

            services.AddScoped<UnitOfWork>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBehaviourDefinitionRepository, BehaviourDefinitionRepository>();
            services.AddScoped<IBehaviourTriggerRepository, BehaviourTriggerRepository>();
            services.AddScoped<IBehaviourEffectRepository, BehaviourEffectRepository>();
            services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
        }
    }
}
