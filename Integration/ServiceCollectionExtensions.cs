using Efeu.Integration.Commands;
using Efeu.Integration.Model;
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
    public static class ServiceCollectionExtensions
    {
        public static void AddEfeu(this IServiceCollection services)
        {
            services.AddScoped<IBehaviourDefinitionCommands, BehaviourDefinitionCommands>();
            services.AddScoped<IBehaviourTriggerCommands, BehaviourTriggerCommands>();
            services.AddScoped<IBehaviourEffectCommands, BehaviourEffectCommands>();
        }
    }
}
