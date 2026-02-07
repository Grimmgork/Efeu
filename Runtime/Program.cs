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

            BehaviourDefinitionStep[] steps = [
                new () {
                    Type = BehaviourStepType.Let,
                    Name = "Value",
                    Input = BehaviourDefinitionExpression.Eval(1),
                },
                new () {
                    Type = BehaviourStepType.Emit,
                    Name = "HelloWorld"
                },
                new () {
                    Type = BehaviourStepType.Await,
                    Name = "Event",
                    Do = [
                        new () {
                            Type = BehaviourStepType.If,
                            Input = BehaviourDefinitionExpression.Eval(1),
                            Do = [
                                new () {
                                    Type = BehaviourStepType.Emit,
                                    Name = "HelloWorld1"
                                }
                            ],
                            Else = [
                                new () {
                                    Type = BehaviourStepType.Emit,
                                    Name = "HelloWorld2"
                                }
                            ]
                        },
                    ]
                },
                new () {
                    Type = BehaviourStepType.Emit,
                    Name = "HelloWorld"
                },
            ];

            EfeuRuntime behaviour1 = EfeuRuntime.Run(steps, Guid.NewGuid(), 10);
            EfeuTrigger trigger = behaviour1.Triggers.First();

            EfeuRuntime behaviour2 = EfeuRuntime.RunTrigger(trigger, new EfeuMessage()
            {
                 Tag = EfeuMessageTag.Data,
                 Name = "Event"
            });

            Console.WriteLine("done");
        }
    }
}
