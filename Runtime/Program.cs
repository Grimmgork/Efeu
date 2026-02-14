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
            //EfeuScriptScope scope = EfeuScriptScope.Empty
            //    .With("time", DateTime.UtcNow)
            //    .With("time", DateTime.Now);

            //string script = "time";
            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.Converters.Add(new EfeuValueJsonConverter());
            //options.Converters.Add(new JsonStringEnumConverter());

            //EfeuValue value = EfeuScript.Run(script, scope);
            //Console.WriteLine(JsonSerializer.Serialize(value, options));

            EfeuBehaviourStep[] steps = [
                new () {
                    Kind = EfeuBehaviourStepKind.Let,
                    Name = "Value",
                    Input = new () {
                        Type = EfeuExpressionType.Struct,
                        Fields = {
                            ["a"] = new () { Type = EfeuExpressionType.Boolean, Value = true },
                            ["b"] = new () { Type = EfeuExpressionType.Boolean, Value = true },
                            ["c"] = new () { Type = EfeuExpressionType.Boolean, Value = true },
                        }
                    },
                },
                new () {
                    Kind = EfeuBehaviourStepKind.Emit,
                    Name = "HelloWorld"
                },
                new () {
                    Kind = EfeuBehaviourStepKind.On,
                    Name = "Event",
                    Do = [
                        new () {
                            Kind = EfeuBehaviourStepKind.If,
                            Input = new () { Type = EfeuExpressionType.Boolean, Value = true },
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

            EfeuRuntimeSimulation simulation = EfeuRuntimeSimulation.Run(steps);
            simulation.Send(new EfeuMessage()
            {
                Id = Guid.NewGuid(),
                Type = "Event",
                Timestamp = DateTime.Now,
                Tag = EfeuMessageTag.Data
            });

            return;

            //EfeuRuntime runtime1 = EfeuRuntime.Run(steps, 10);
            //EfeuTrigger trigger = runtime1.Triggers.First();

            //EfeuRuntime runtime2 = EfeuRuntime.RunTrigger(trigger, new EfeuMessage()
            //{
            //     Tag = EfeuMessageTag.Data,
            //     Type = "Event"
            //});

            //Console.WriteLine("done");
        }
    }
}
