using Efeu.Integration.Commands;
using Efeu.Integration.Foreign;
using Efeu.Integration.Model;
using Efeu.Integration.Services;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{

    public interface IDefaultEffectProvider : IEffectProvider
    {
        public void Register(string name, Func<IServiceProvider, IEffect> factory);
    }

    public class DefaultEffectProvider : IEffectProvider
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultEffectProvider(IServiceProvider services)
        {
            this.serviceProvider = services;
        }

        public bool IsEffect(string name)
        {
            return name.StartsWith("_");
        }

        public IEffect? TryGetEffect(string name)
        {
            Func<IServiceProvider, IEffect>? factory = serviceProvider.GetKeyedService<Func<IServiceProvider, IEffect>>(name);
            if (factory == null)
                return null;

            return factory(serviceProvider);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static void AddEfeu(this IServiceCollection services)
        {
            services.AddScoped<IBehaviourDefinitionCommands, BehaviourDefinitionCommands>();
            services.AddScoped<IBehaviourTriggerCommands, BehaviourTriggerCommands>();
            services.AddScoped<IBehaviourEffectCommands, BehaviourEffectCommands>();
            services.AddScoped<IEffectProvider, DefaultEffectProvider>();
            services.AddHostedService<EffectExecutionService>();
            services.AddScoped<EfeuEnvironment>();
        }
    }
}
