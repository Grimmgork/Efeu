using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    internal class DefaultEfeuEffectProvider : IEfeuEffectProvider
    {
        private readonly IServiceProvider services;

        public DefaultEfeuEffectProvider(IServiceProvider serviceProvider) {
            this.services = serviceProvider;
        }

        public bool IsEffect(string name)
        {
            object? match = services.GetKeyedService<IEffect>(name);
            return match is not null;
        }

        public IEffect? TryGetEffect(string name)
        {
            return services.GetKeyedService<IEffect>(name);
        }
    }
}
