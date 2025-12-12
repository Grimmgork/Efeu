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
            services.AddScoped<IBehaviourTriggerCommands, BehaviourTriggerCommands>();
            services.AddScoped<IBehaviourEffectCommands, BehaviourEffectCommands>();
            services.AddScoped<IEffectProvider, DefaultEffectProvider>();
            services.AddHostedService<SignalProcessingService>();
            services.AddHostedService<EffectExecutionService>();
            services.AddScoped<EfeuEnvironment>();
        }

        public static void AddEfeuDefaultEffects(this IServiceCollection services)
        {
            services.AddEfeuEffect<WriteConsoleEffect>("WriteConsole");
        }

        public static void AddEfeuEffect<T>(this IServiceCollection services, string name) where T : IEffect
        {
            services.AddKeyedTransient(typeof(IEffect), name, typeof(T));
        }
    }
}
