// See https://aka.ms/new-console-template for more information
namespace Efeu.Runtime;

using System;
using System.Collections;
using System.IO;
using System.Linq;
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
    static async Task Main(string[] args)
    {
        WorkflowDefinition definition = new WorkflowDefinition("workflow", 2);
        //definition.Method(1, "WriteVariable")
        //    .Input("Name", InputSource.Literal("Variable1"))
        //    .Input("Value", InputSource.Literal(SomeData.Null()))
        //    .Then(2);

        definition.Method(2, "WriteVariable")
            .Input("Name", InputSource.Literal("Variable1"))
            .Input("Value", InputSource.FunctionOutput(4))
            .Then(3);

        definition.Method(3, "Print")
            .Input("Message", InputSource.Variable("Variable1"))
            .Then(2);

        definition.Function(4, "+")
            .Input("A", InputSource.Variable("Variable1"))
            .Input("B", InputSource.Literal(1));

        //definition.Method(1, "Print")
        //    .Input("Message", InputSource.Literal("Enter a message ..."))
        //    .Then(2);

        //definition.Method(2, "WriteVariable")
        //    .Input("Name", InputSource.Literal("MyVariable"))
        //    .Input("Value", InputSource.Literal(
        //        SomeData.Struct([
        //            new("A", 42),
        //            new("B", true)
        //        ])))
        //    .Then(3);

        //definition.Method(3, "WaitForInput")
        //    .Then(4)
        //    .Error(5);

        //definition.Method(4, "Print")
        //    .Input("Message", InputSource.FunctionOutput(7))
        //    .Then(6);

        //definition.Method(6, "SetOutput")
        //    .Input("Name", InputSource.Literal("MyOutput"))
        //    .Input("Value", InputSource.FunctionOutput(7));

        //definition.Method(5, "Print")
        //    .Input("Message", InputSource.Literal("An Error occured!"));

        //definition.Function(7, "Map")
        //    .Input("Array", InputSource.Literal(SomeData.Array([1, 2, 3, 4])))
        //    // .Do((input) => SomeData.Integer(input.ToInt32() + 1));
        //    .Do(8);

        //definition.Function(8, "+")
        //    .Input("A", InputSource.LambdaInput())
        //    .Input("B", InputSource.Literal(1));

        //var options = new JsonSerializerOptions()
        //{
        //    WriteIndented = true
        //};
        //options.Converters.Add(new InterfaceJsonConverter<IInputSource>(
        //    typeof(FunctionOutput), typeof(MethodOutput), typeof(Variable), typeof(Literal)));
        //options.Converters.Add(new SomeDataJsonConverter());
        //options.Converters.Add(new SomeDataTraversalJsonConverter());

        //File.WriteAllText("workflow.json", JsonSerializer.Serialize(definition, options));
        //definition = JsonSerializer.Deserialize<WorkflowDefinition>(File.ReadAllText("workflow.json"), options)!;

        DefaultWorkflowFunctionInstanceFactory instanceFactory = new DefaultWorkflowFunctionInstanceFactory();
        instanceFactory.Register("WriteVariable", () => new WriteVariableMethod());
        instanceFactory.Register("Print", () => new PrintMethod());
        instanceFactory.Register("WaitForInput", () => new WaitForInputMethod());
        instanceFactory.Register("If", () => new IfMethod());
        instanceFactory.Register("SetOutput", () => new SetOutputMethod());
        instanceFactory.Register("If", () => new WorkflowFunction((inputs) => inputs["Condition"].ToBoolean() ? inputs["Then"] : inputs["Else"]));
        instanceFactory.Register("+", () => new WorkflowFunction((inputs) => SomeData.Integer(
            (inputs["A"].ToDynamic() ?? 0) + 
            (inputs["B"].ToDynamic() ?? 0)
        )));
        instanceFactory.Register("Map", () => new WorkflowFunction((context, input) => SomeData.Array(input["Array"].Items.Select(i => context.ComputeLambda(i)))));
        instanceFactory.Register("GetGuid", () => new GetGuid());

        WorkflowInstance instance = new WorkflowInstance(1, definition, instanceFactory);

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
        SomeData array = data.Output["MyOutput"];
        Console.WriteLine(array.DataType);
        foreach (SomeData item in array.Items)
        {
            Console.WriteLine(item.ToInt32());
        }
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

