using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Efeu.Runtime.Value;
using Efeu.Runtime.Json.Converters;
using Efeu.Runtime.Script;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Efeu.Runtime
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EfeuScriptScope scope = EfeuScriptScope.Empty
                .With("time", DateTime.UtcNow)
                .With("time", DateTime.Now);

            string script = "time";
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            EfeuValue value = EfeuScript.Run(script, scope);
            Console.WriteLine(JsonSerializer.Serialize(value, options));
            return;

            EfeuBehaviourStep[] steps = [
                new () {
                    Kind = EfeuBehaviourStepKind.Let,
                    Name = "Value",
                    Input = EfeuBehaviourExpression.Eval(1),
                },
                new () {
                    Kind = EfeuBehaviourStepKind.Emit,
                    Name = "HelloWorld"
                },
                new () {
                    Kind = EfeuBehaviourStepKind.Await,
                    Name = "Event",
                    Do = [
                        new () {
                            Kind = EfeuBehaviourStepKind.If,
                            Input = EfeuBehaviourExpression.Eval(1),
                            Do = [
                                new () {
                                    Kind = EfeuBehaviourStepKind.Emit,
                                    Name = "HelloWorld1"
                                }
                            ],
                            Else = [
                                new () {
                                    Kind = EfeuBehaviourStepKind.Emit,
                                    Name = "HelloWorld2"
                                }
                            ]
                        },
                    ]
                },
                new () {
                    Kind = EfeuBehaviourStepKind.Emit,
                    Name = "HelloWorld"
                },
            ];

            EfeuRuntime runtime1 = EfeuRuntime.Run(steps, Guid.NewGuid(), 10);
            EfeuTrigger trigger = runtime1.Triggers.First();

            EfeuRuntime runtime2 = EfeuRuntime.RunTrigger(trigger, new EfeuMessage()
            {
                 Tag = EfeuMessageTag.Data,
                 Type = "Event"
            });

            Console.WriteLine("done");
        }
    }
}
