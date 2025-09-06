// See https://aka.ms/new-console-template for more information
namespace Efeu.Runtime;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu.Integration.Model;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Json;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Trigger;

class Program
{
    static async Task Main(string[] args)
    {
        WorkflowTriggerHash[] array = [new WorkflowTriggerHash("test"), new WorkflowTriggerHash("testa")];
        // Console.WriteLine(array.Contains(new WorkflowTriggerHash("testa")));
        // return;


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

        WorkflowRuntimeEnvironment environment = new WorkflowRuntimeEnvironment(methodProvider, functionProvider, triggerProvider);
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

        Console.WriteLine("Done!");

        WorkflowRuntimeExport data = runtime.Export();
        Console.WriteLine(data.Output.ToString());
    }
}

// [inp] Name.Name
// [inp] Name.Name
// [var] Name.Name[0]
// [exp] (Name)

// a name is a series of .Name and/or array indexers [2]

// (inp :name.name)
// (inp :name.name.name[0].name)
// (var :name)
// (:name a b (+ a b))
// (map (inp :name.name) (: a b (+ a b)))
// (+ 1 2)
// (name 1 2)
// (name 1 2)
// (name 2 3)

