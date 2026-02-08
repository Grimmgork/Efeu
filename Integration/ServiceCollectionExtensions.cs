using Efeu.Integration.Commands;
using Efeu.Integration.Foreign;
using Efeu.Integration.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEfeu(this IServiceCollection services)
        {
            services.AddScoped<IBehaviourDefinitionCommands, BehaviourDefinitionCommands>();
            services.AddScoped<ITriggerCommands, TriggerCommands>();
            services.AddScoped<IEffectCommands, EffectCommands>();
            services.AddScoped<IDeduplicationKeyCommands, DeduplicationKeyCommands>();
            services.AddScoped<IEfeuEffectProvider, DefaultEfeuEffectProvider>();
            services.AddScoped<IEfeuTriggerProvider, DefaultEfeuTriggerProvider>();
            services.AddScoped<IEfeuEngine, EfeuEngine>();
            services.AddHostedService<EffectExecutionService>();
        }

        public static void AddEfeuDefaultEffects(this IServiceCollection services)
        {
            services.AddEfeuEffect<WriteConsoleEffect>("WriteConsole");
        }

        public static void AddEfeuEffect<T>(this IServiceCollection services, string name) where T : IEfeuEffect
        {
            services.AddKeyedTransient(typeof(IEfeuEffect), name, typeof(T));
        }

        public static void AddEfeuTrigger<T>(this IServiceCollection services, string name) where T : IEfeuTrigger
        {
            services.AddKeyedTransient(typeof(IEfeuTrigger), name, typeof(T));
        }
    }
}
