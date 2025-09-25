// See https://aka.ms/new-console-template for more information
namespace Efeu.Runtime;

using Antlr4.Runtime;
using Efeu.Integration.Model;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.JSON;
using Efeu.Runtime.JSON.Converters;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Serialization;
using Efeu.Runtime.Trigger;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var input = "call SomeInterop with";
        var inputStream = new AntlrInputStream(input);
        var lexer = new EfeuGrammarLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new EfeuGrammarParser(tokens);

        var tree = parser.line(); // start rule

        Console.WriteLine(tree.ToStringTree(parser));
        return;

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new EfeuValueJsonConverter());
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
        functionProvider.Register("If", () => new WorkflowFunction((input) => input.Call("Condition").ToBool() ? input.Call("Then") : input.Call("Else")));
        functionProvider.Register("+", () => new WorkflowFunction((input) => input.Call("A").ToDecimal() + input.Call("B").ToDecimal()));
        functionProvider.Register("Eval", () => new WorkflowFunction((input) => input));
        functionProvider.Register("Filter", () => new WorkflowFunction((context, input) => new EfeuArray(input.Each().Where((i) => context.Yield(i).ToBool()))));
        functionProvider.Register("Map", () => new WorkflowFunction((context, input) => new EfeuArray(input.Each().Select(context.Yield))));

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

        Console.WriteLine("DONE:");
        WorkflowRuntimeExport export = runtime.Export();
        IWorkflowRuntimeExportSerializer serializer = new WorkflowRuntimeExportMessagePackSerializer();

        byte[] bytes = serializer.Serialize(export);
        // Console.WriteLine(Convert.ToBase64String(bytes));
        WorkflowRuntimeExport clone = serializer.Deserialize(bytes);

        Console.WriteLine("OUTPUT:");
        Console.WriteLine(runtime.Output.ToString());
    }
}
