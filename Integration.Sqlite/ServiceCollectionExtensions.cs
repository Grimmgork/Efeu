using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Runtime.Value;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Efeu.Integration.Sqlite.Queries;
using Efeu.Runtime;
using Efeu.Runtime.Json.Converters;
using LinqToDB.DataProvider.SQLite;
using System.Data.SQLite;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Efeu.Integration.Sqlite;

public static class ServiceCollectionExtensions
{
    private static T ConvertFromJson<T>(string json, JsonSerializerOptions jsonOptions)
    {
        return JsonSerializer.Deserialize<T>(json, jsonOptions)!;
    }
    
    private static string ConvertToJson<T>(T value, JsonSerializerOptions jsonOptions)
    {
        return JsonSerializer.Serialize(value, jsonOptions);
    }
    
    private static void RegisterJsonConversion<T>(
        MappingSchema mappingSchema,
        JsonSerializerOptions jsonOptions)
    {
        mappingSchema.AddScalarType(typeof(T), DataType.Text);
        
        mappingSchema.SetConverter<T, string>(
            value => ConvertToJson(value, jsonOptions));
        
        mappingSchema.SetConverter<string, T>(
            value => ConvertFromJson<T>(value, jsonOptions));
        
        mappingSchema.SetConvertExpression<T, DataParameter>(
            value => DataParameter.Text(
                null,
                ConvertToJson(value, jsonOptions)));
    }

    private static MappingSchema ConfigureMappingSchema(string schema)
    {
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions();
        jsonOptions.IncludeFields = true;
        jsonOptions.Converters.Add(new EfeuValueJsonConverter());
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.Converters.Add(new EfeuRuntimeScopeJsonConverter());
        
        FluentMappingBuilder builder = new FluentMappingBuilder();

        RegisterJsonConversion<EfeuValue>(builder.MappingSchema, jsonOptions);
        RegisterJsonConversion<Dictionary<int, EfeuValue>>(builder.MappingSchema, jsonOptions);
        RegisterJsonConversion<Stack<int>>(builder.MappingSchema, jsonOptions);
        RegisterJsonConversion<EfeuBehaviourStep[]>(builder.MappingSchema, jsonOptions);
        RegisterJsonConversion<EfeuRuntimeScope>(builder.MappingSchema, jsonOptions);
        RegisterJsonConversion<ImmutableDictionary<string, EfeuValue>>(builder.MappingSchema, jsonOptions);
        
        builder.MappingSchema.AddScalarType(typeof(DateTimeOffset), DataType.Int64);
        builder.MappingSchema.SetConverter<DateTimeOffset, long>(c => c.ToUnixTimeMilliseconds());
        builder.MappingSchema.SetConverter<long, DateTimeOffset>(DateTimeOffset.FromUnixTimeMilliseconds);
        builder.MappingSchema.SetConvertExpression<DateTimeOffset, DataParameter>(
            value => DataParameter.Int64(
                null,
                value.ToUnixTimeMilliseconds()));
        
        builder.Entity<BehaviourEntity>()
            .HasTableName("Behaviour")
            .HasSchemaName(schema)
            .Property(p => p.Id)
                .IsIdentity()
                .IsPrimaryKey()
            .Property(p => p.Name)
            .Property(p => p.Version);

        builder.Entity<BehaviourVersionEntity>()
            .HasTableName("BehaviourVersion")
            .HasSchemaName(schema)
            .Property(p => p.Id)
                .IsIdentity()
                .IsPrimaryKey()
                .HasSkipOnInsert(false)
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
            .Property(p => p.BehaviourVersionId)
            .Property(p => p.Type)
            .Property(p => p.Tag)
            .Property(p => p.Position)
            .Property(p => p.ScopeId)
            .Property(p => p.Matter)
            .Property(p => p.Group)
            .Property(p => p.IsDetatched);

        builder.Entity<EffectEntity>()
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
            .Property(p => p.Type)
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

        builder.Entity<BehaviourScopeEntity>()
            .HasTableName("BehaviourScope")
            .HasSchemaName(schema)
            .Property(p => p.Id)
                .IsIdentity()
                .IsPrimaryKey()
                .HasSkipOnInsert(false)
            .Property(p => p.ReferenceCount)
            .Property(p => p.Constants);

        builder.Entity<ValueNodeEntity>()
            .HasTableName("ValueNode")
            .HasSchemaName(schema)
            .Property(p => p.Hash)
                .IsIdentity()
                .IsPrimaryKey()
                .HasSkipOnInsert(false)
            .Property(p => p.Payload);
        
        builder.Entity<ValueNodeReferenceEntity>()
            .HasTableName("ValueNodeReference")
            .HasSchemaName(schema)
            .Property(p => p.SourceHash)
            .Property(p => p.TargetHash);

        builder.Build();

        return builder.MappingSchema;
    }
    
    public static void AddEfeuSqlite(this IServiceCollection services, string schema, string connectionString)
    {
        MappingSchema mappingSchema = ConfigureMappingSchema(schema);
        services.AddScoped((serviceProvider) =>
        {
            var options = new DataOptions()
                .UseSQLite(connectionString)
                .UseMappingSchema(mappingSchema)
                .UseAfterConnectionOpened(AfterConnectionOpened);
            return new DataConnection(options);
        });

        services.AddEfeuSqliteServices();
    }
    
    public static void AddEfeuSqlite(this IServiceCollection services, string schema)
    {
        MappingSchema mappingSchema = ConfigureMappingSchema(schema);
        services.AddScoped((serviceProvider) =>
        {
            var options = new DataOptions()
                .UseDataProvider(SQLiteTools.GetDataProvider(SQLiteProvider.System))
                .UseConnection(serviceProvider.GetRequiredService<SQLiteConnection>())
                .UseMappingSchema(mappingSchema)
                .UseAfterConnectionOpened(AfterConnectionOpened);
            return new DataConnection(options);
        });

        services.AddEfeuSqliteServices();
    }
    
    private static void AfterConnectionOpened(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON";
        command.ExecuteNonQuery();
    }

    private static void AddEfeuSqliteServices(this IServiceCollection services)
    {
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IEfeuUnitOfWork, UnitOfWork>();
        services.AddScoped<IBehaviourQueries, BehaviourQueries>();
        services.AddScoped<ITriggerQueries, TriggerQueries>();
        services.AddScoped<IEffectQueries, EffectQueries>();
        services.AddScoped<IEfeuMigrationRunner, MigrationRunner>();
        services.AddScoped<IDeduplicationKeyQueries, DeduplicationKeyQueries>();
        services.AddScoped<IBehaviourScopeQueries, BehaviourScopeQueries>();
        services.AddScoped<IValueNodeQueries, ValueNodeQueries>();
    }
}
