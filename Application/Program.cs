using Efeu.Integration;
using Efeu.Integration.Persistence;
using Efeu.Integration.Sqlite;
using Efeu.Runtime.JSON.Converters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();

            builder.Services.AddControllersWithViews()
                     .AddJsonOptions(options =>
                     {
                         options.JsonSerializerOptions.IncludeFields = true;
                         options.JsonSerializerOptions.Converters.Add(new EfeuValueJsonConverter());
                         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                     });

            builder.Services.AddEfeu();
            builder.Services.AddEfeuDefaultEffects();
            builder.Services.AddEfeuSqlite("Data Source=data.db");

            var app = builder.Build();

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Console.WriteLine("running migrations ...");
            app.MigrateEfeuAsync().GetAwaiter().GetResult();
            Console.WriteLine("done");
            
            app.Run();
        }
    }
}
