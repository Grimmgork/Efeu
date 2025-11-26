using Efeu.Integration.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public static class HostExtensions
    {
        public static Task MigrateEfeuAsync(this IHost host, int version = 0)
        {
            using (var scope = host.Services.CreateScope())
            {
                IEfeuMigrationRunner migrationRunner = scope.ServiceProvider.GetRequiredService<IEfeuMigrationRunner>();
                return migrationRunner.MigrateToAsync(version);
            }
        }
    }
}
