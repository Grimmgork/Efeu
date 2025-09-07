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
        public static void AddEfeuFunctions(this IServiceCollection services, Func<IServiceProvider, Task<IEnumerable<WorkflowFunctionDescription>>> functions)
        {
            services.AddScoped(functions);
        }

        public static void AddEfeuMethods(this IServiceCollection services, Func<IServiceProvider, Task<IEnumerable<WorkflowMethodDescription>>> methods)
        {
            services.AddScoped(methods);
        }

        public static void AddEfeuFunctions(this IServiceCollection services, Func<IServiceProvider, IEnumerable<WorkflowFunctionDescription>> functions)
        {
            services.AddScoped((services) => Task.FromResult(functions(services)));
        }

        public static void AddEfeuMethods(this IServiceCollection services, Func<IServiceProvider, IEnumerable<WorkflowMethodDescription>> methods)
        {
            services.AddScoped((services) => Task.FromResult(methods(services)));
        }

        public static void AddEfeuFunction(this IServiceCollection services, WorkflowFunctionDescription function)
        {
            services.AddSingleton(function);
        }

        public static void AddEfeuMethod(this IServiceCollection services, WorkflowMethodDescription method)
        {
            services.AddSingleton(method);
        }

        public static void AddEfeuMethod(this IServiceCollection services, Func<IServiceProvider, WorkflowMethodDescription> factory)
        {
            services.AddSingleton(factory);
        }

        public static void AddEfeuFunction(this IServiceCollection services, Func<IServiceProvider, WorkflowFunctionDescription> factory)
        {
            services.AddSingleton(factory);
        }

        public static void AddEfeu(this IServiceCollection services)
        {
            //SimpleWorkflowMethodProvider methodProvider = new SimpleWorkflowMethodProvider();
            //methodProvider.Register("ForEach", () => new ForeachMethod());
            //methodProvider.Register("Print", () => new PrintMethod());
            //methodProvider.Register("WaitForInput", () => new WaitForInputMethod());
            //methodProvider.Register("If", () => new IfMethod());
            //methodProvider.Register("Filter", () => new FilterMethod());
            //methodProvider.Register("Eval", () => new EvalMethod());
            //methodProvider.Register("GetGuid", () => new GetGuid());
            //methodProvider.Register("Times", () => new TimesMethod());

            //SimpleWorkflowFunctionProvider functionProvider = new SimpleWorkflowFunctionProvider();
            //functionProvider.Register("If", () => new WorkflowFunction((input) => input["Condition"].ToBoolean() ? input["Then"] : input["Else"]));
            //functionProvider.Register("+", () => new WorkflowFunction((input) => SomeData.Parse(
            //    (dynamic)(input["A"].Value ?? 0) +
            //    (dynamic)(input["B"].Value ?? 0)
            //)));
            //functionProvider.Register("Eval", () => new WorkflowFunction((input) => input));

            //SimpleWorkflowTriggerProvider triggerProvider = new SimpleWorkflowTriggerProvider();
            //// triggerProvider.Register("Cron", () => new CronTrigger())

            //services.AddScoped<IWorkflowMethodProvider, SimpleWorkflowMethodProvider>();
            //services.AddScoped<IWorkflowFunctionProvider, SimpleWorkflowFunctionProvider>();
            //services.AddScoped<IWorkflowTriggerProvider, SimpleWorkflowTriggerProvider>();

            services.AddScoped<IWorkflowDefinitionCommands, WorkflowDefinitionCommands>();
            services.AddScoped<IWorkflowInstanceCommands, WorkflowInstanceCommands>();
        }
    }
}
