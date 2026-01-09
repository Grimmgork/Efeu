using Efeu.Integration;
using Efeu.Integration.Sqlite;
using Efeu.Router.Json.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu.Integration.Quartz;

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

            builder.Services.AddScoped((services) =>
            {
                SQLiteConnection connection = new SQLiteConnection("Data Source=data.db");
                return connection;
            });

            builder.Services.AddEfeu();
            builder.Services.AddEfeuDefaultEffects();
            builder.Services.AddEfeuQuartz();
            // builder.Services.AddEfeuSqlite("efeu", "Data Source=data.db");
            builder.Services.AddEfeuSqlite("efeu");

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
