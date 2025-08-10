// See https://aka.ms/new-console-template for more information
namespace Efeu.Runtime;

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Json;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Signal;

class Program
{
    static async Task Main(string[] args)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.Converters.Add(new SomeDataJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());

        WorkflowDefinition definition = JsonSerializer.Deserialize<WorkflowDefinition>(File.ReadAllText("workflow.json"), options)!;

        DefaultWorkflowActionInstanceFactory instanceFactory = new DefaultWorkflowActionInstanceFactory();
        instanceFactory.Register("For", () => new ForMethod());
        instanceFactory.Register("ForEach", () => new ForeachMethod());
        instanceFactory.Register("WriteVariable", () => new WriteVariableMethod());
        instanceFactory.Register("Print", () => new PrintMethod());
        instanceFactory.Register("WaitForInput", () => new WaitForInputMethod());
        instanceFactory.Register("If", () => new IfMethod());
        instanceFactory.Register("If", () => new WorkflowFunction((input) => input["Condition"].ToBoolean() ? input["Then"] : input["Else"]));
        instanceFactory.Register("+", () => new WorkflowFunction((input) => SomeData.Parse(
            (input["A"].ToDynamic() ?? 0) + 
            (input["B"].ToDynamic() ?? 0) 
        )));
        instanceFactory.Register("Eval", () => new WorkflowFunction((input) => input));
        instanceFactory.Register("Map", () => new MapMethod());
        instanceFactory.Register("Filter", () => new FilterMethod());
        instanceFactory.Register("Eval", () => new EvalMethod());
        instanceFactory.Register("GetGuid", () => new GetGuid());
        instanceFactory.Register("SetVariable", () => new SetVariableMethod());

        WorkflowInstance instance = new WorkflowInstance(definition, instanceFactory);

        do
        {
            if (instance.State == WorkflowInstanceState.Suspended)
            {
                string message = Console.ReadLine() ?? "";
                instance.SendSignal(new CustomWorkflowSignal()
                {
                    Name = "Message",
                    Timestamp = DateTime.Now,
                    Payload = message
                });
            }

            await instance.RunAsync();
        }
        while (instance.State != WorkflowInstanceState.Done);
        Console.WriteLine("Done!");

        WorkflowInstanceExport data = instance.Export();
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

