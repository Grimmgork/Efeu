// See https://aka.ms/new-console-template for more information
namespace Efeu.Runtime;

using Efeu.Integration.Model;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Json;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Trigger;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        EfeuValue value = EfeuValue.Array([1, 2, 3]);
        value.Push(42);
        value.As<EfeuArray>();
        value.Call(5, 42);
        value.Call(10);


        value.Each((value) => Console.WriteLine(value));

        return;

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new SomeDataJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());
        
        WorkflowDefinition definition = JsonSerializer.Deserialize<WorkflowDefinition>(File.ReadAllText("workflow.json"), options)!;

        SimpleWorkflowMethodProvider methodProvider = new SimpleWorkflowMethodProvider();
        methodProvider.Register("ForEach", () => new ForeachMethod());
        methodProvider.Register("Print", () => new PrintMethod());
        methodProvider.Register("WaitForInput", () => new WaitForInputMethod());
        methodProvider.Register("If", () => new IfMethod());
        methodProvider.Register("Filter", () => new FilterMethod());
        methodProvider.Register("Eval", () => new EvalMethod());
        methodProvider.Register("GetGuid", () => new GetGuid());
        methodProvider.Register("Times", () => new TimesMethod());

        SimpleWorkflowFunctionProvider functionProvider = new SimpleWorkflowFunctionProvider();
        functionProvider.Register("If", () => new WorkflowFunction((input) => input["Condition"].ToBoolean() ? input["Then"] : input["Else"]));
        functionProvider.Register("+", () => new WorkflowFunction((input) => SomeData.Parse(
            (dynamic)(input["A"].Value ?? 0) +
            (dynamic)(input["B"].Value ?? 0) 
        )));
        functionProvider.Register("Eval", () => new WorkflowFunction((input) => input));

        SimpleWorkflowTriggerProvider triggerProvider = new SimpleWorkflowTriggerProvider();
        // triggerProvider.Register("Cron", () => new CronTrigger())

        ConsoleWorkflowOutbox outbox = new ConsoleWorkflowOutbox();

        WorkflowRuntimeEnvironment environment = new WorkflowRuntimeEnvironment(methodProvider, functionProvider, triggerProvider, outbox);
        WorkflowRuntime runtime = WorkflowRuntime.Prepare(environment, definition);

        await runtime.RunAsync();
        while (runtime.State == WorkflowRuntimeState.Suspended)
        {
            string message = Console.ReadLine() ?? "";
            await runtime.ContinueAsync(new WorkflowTriggerHash(nameof(ConsoleInputSignal)), new ConsoleInputSignal()
            {
                Input = message
            });
        }

        Console.WriteLine("OUTPUT:");
        Console.WriteLine(runtime.Output.ToString());
    }
}
