using Efeu.Integration.Commands;
using Efeu.Integration.Foreign;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Quartz
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEfeuQuartz(this IServiceCollection services)
        {
            services.AddEfeuTrigger<CronTrigger>("Cron");
            services.AddQuartzHostedService(
                    q => q.WaitForJobsToComplete = true);
            services.AddQuartz(q =>
            {
                // Use a Scoped container to create jobs. I'll touch on this later
                q.UseInMemoryStore();
            });
        }
    }
}
