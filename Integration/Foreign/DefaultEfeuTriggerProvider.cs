using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    internal class DefaultEfeuTriggerProvider : IEfeuTriggerProvider
    {
        private readonly IServiceProvider services;

        public DefaultEfeuTriggerProvider(IServiceProvider serviceProvider)
        {
            this.services = serviceProvider;
        }

        public IEfeuTrigger? TryGetTrigger(string name)
        {
            return services.GetKeyedService<IEfeuTrigger>(name);
        }
    }
}
