using Efeu.Integration.Commands;
using Efeu.Integration.Model;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.AddEfeuMethod((provider) => [ ]);

            SimpleWorkflowFunctionProvider defaultFactory = new SimpleWorkflowFunctionProvider();
            defaultFactory.Register("ForEach", () => new ForeachMethod());
            defaultFactory.Register("Print", () => new PrintMethod());
            defaultFactory.Register("WaitForInput", () => new WaitForInputMethod());
            defaultFactory.Register("If", () => new IfMethod());
            defaultFactory.Register("If", () => new WorkflowFunction((input) => input["Condition"].ToBoolean() ? input["Then"] : input["Else"]));
            defaultFactory.Register("+", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) +
                (dynamic)(input["B"].Value ?? 0)
            )));
            defaultFactory.Register("-", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) -
                (dynamic)(input["B"].Value ?? 0)
            )));
            defaultFactory.Register("/", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) /
                (dynamic)(input["B"].Value ?? 0)
            )));
            defaultFactory.Register("*", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) *
                (dynamic)(input["B"].Value ?? 0)
            )));
            defaultFactory.Register("%", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) %
                (dynamic)(input["B"].Value ?? 0)
            )));
            defaultFactory.Register("Filter", () => new FilterMethod());
            defaultFactory.Register("Eval", () => new EvalMethod());
            defaultFactory.Register("GetGuid", () => new GetGuid());

            services.AddScoped<IWorkflowDefinitionCommands, WorkflowDefinitionCommands>();
            services.AddScoped<IWorkflowInstanceCommands, WorkflowInstanceCommands>();
        }
    }
}
