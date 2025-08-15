using Efeu.Integration.Commands;
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
        public static Task InitializeEfeu(this IServiceProvider services)
        {
            throw new Exception();
        }

        public static void AddEfeu(this IServiceCollection services)
        {
            DefaultWorkflowActionInstanceFactory instanceFactory = new DefaultWorkflowActionInstanceFactory();
            instanceFactory.Register("ForEach", () => new ForeachMethod());
            instanceFactory.Register("Print", () => new PrintMethod());
            instanceFactory.Register("WaitForInput", () => new WaitForInputMethod());
            instanceFactory.Register("If", () => new IfMethod());
            instanceFactory.Register("If", () => new WorkflowFunction((input) => input["Condition"].ToBoolean() ? input["Then"] : input["Else"]));
            instanceFactory.Register("+", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) +
                (dynamic)(input["B"].Value ?? 0)
            )));
            instanceFactory.Register("-", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) -
                (dynamic)(input["B"].Value ?? 0)
            )));
            instanceFactory.Register("/", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) /
                (dynamic)(input["B"].Value ?? 0)
            )));
            instanceFactory.Register("*", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) *
                (dynamic)(input["B"].Value ?? 0)
            )));
            instanceFactory.Register("%", () => new WorkflowFunction((input) => SomeData.Parse(
                (dynamic)(input["A"].Value ?? 0) %
                (dynamic)(input["B"].Value ?? 0)
            )));
            instanceFactory.Register("Filter", () => new FilterMethod());
            instanceFactory.Register("Eval", () => new EvalMethod());
            instanceFactory.Register("GetGuid", () => new GetGuid());

            services.AddScoped<IWorkflowActionInstanceFactory>((services) => instanceFactory);
            services.AddScoped<IWorkflowDefinitionCommands, WorkflowDefinitionCommands>();
            services.AddScoped<IWorkflowInstanceCommands, WorkflowInstanceCommands>();
        }
    }
}
