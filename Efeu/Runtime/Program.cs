// See https://aka.ms/new-console-template for more information
namespace Efeu.Runtime;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Efeu;
using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Json;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Signal;

class Program
{
    static async Task MainAsync(string[] args)
    {
        WorkflowDefinition definition = new WorkflowDefinition("workflow", 1);
        definition.Method(1, "Print")
            .Input("Message", InputSource.Literal(SomeData.String("Enter a message ...")))
            .Then(2);

        definition.Method(2, "WriteVariable")
            .Input("Name", InputSource.Literal(SomeData.String("MyVariable")))
            .Input("Value", InputSource.Literal(
                SomeData.Struct([
                    new("A", SomeData.Integer(42)),
                    new("B", SomeData.Boolean(true))])))
            .Then(3);

        definition.Method(3, "WaitForInput")
            .Then(4)
            .Error(5);

        definition.Method(4, "Print")
            .Input("Message", InputSource.Variable("MyVariable.C"))
            .Then(6);

        definition.Method(6, "SetOutput")
            .Input("Name", InputSource.Literal(SomeData.String("MyOutput")))
            .Input("Value", InputSource.Variable("MyVariable.B"));

        definition.Method(5, "Print")
            .Input("Message", InputSource.Literal(SomeData.String("An Error occured!")));

        var options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
        options.Converters.Add(new InterfaceJsonConverter<IInputSource>(
            typeof(FunctionOutput), typeof(MethodOutput), typeof(Variable), typeof(Literal)));
        options.Converters.Add(new SomeDataJsonConverter());
        options.Converters.Add(new SomeDataTraversalJsonConverter());

        File.WriteAllText("workflow.json", JsonSerializer.Serialize(definition, options));
        definition = JsonSerializer.Deserialize<WorkflowDefinition>(File.ReadAllText("workflow.json"), options)!;


        DefaultWorkflowFunctionInstanceFactory instanceFactory = new DefaultWorkflowFunctionInstanceFactory();
        instanceFactory.Register("WriteVariable", () => new WriteVariableMethod());
        instanceFactory.Register("Print", () => new PrintMethod());
        instanceFactory.Register("WaitForInput", () => new WaitForInputMethod());
        instanceFactory.Register("If", () => new IfMethod());
        instanceFactory.Register("SetOutput", () => new SetOutputMethod());
        instanceFactory.Register("If", () => new WorkflowFunction((inputs) => inputs["Condition"].ToBoolean() ? inputs["Then"] : inputs["Else"]));
        instanceFactory.Register("+", () => new WorkflowFunction((inputs) => inputs["A"].ToDynamic() + inputs["B"].ToDynamic()));

        WorkflowInstance instance = new WorkflowInstance(1, definition, instanceFactory, (signal) =>
            Task.Run(() => Console.WriteLine($"SIGNAL: {signal.GetType()}"))
        );

        do
        {
            if (instance.State == WorkflowInstanceState.Suspended)
            {
                string message = Console.ReadLine() ?? "";
                instance.SendSignal(new PromptInputSignal(message, DateTime.Now));
            }

            await instance.RunAsync();
        }
        while (instance.State != WorkflowInstanceState.Done);
        Console.WriteLine("Done!");

        WorkflowInstanceData data = instance.Export();
        Console.WriteLine(data.Output["MyOutput"].ToString());

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

