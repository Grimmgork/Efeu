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
            //EfeuRuntimeScope scope = EfeuRuntimeScope.Empty
            //    .With("time", DateTime.UtcNow)
            //    .With("time", DateTime.Now);

            //// string script = "time";
            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.Converters.Add(new EfeuValueJsonConverter());
            //options.Converters.Add(new EfeuRuntimeScopeJsonConverter());
            //options.Converters.Add(new JsonStringEnumConverter());

            //string serialized = JsonSerializer.Serialize(scope, options);

            //scope = JsonSerializer.Deserialize<EfeuRuntimeScope>(serialized, options);

            //// EfeuValue value = EfeuScript.Run(script, scope);
            //Console.WriteLine();

            //return;

            EfeuBehaviourStep[] steps = [
                new () {
                    Kind = EfeuBehaviourStepKind.Let,
                    Name = "var",
                    Input = new () {
                        Type = EfeuExpressionType.String,
                        Value = "Hello World!"
                    },
                },
                new () {
                    Kind = EfeuBehaviourStepKind.Raise,
                    Name = "A"
                },
                new () {
                    Kind = EfeuBehaviourStepKind.On,
                    Name = "A",
                    Do = [
                        new () {
                            Kind = EfeuBehaviourStepKind.Emit,
                            Name = "WriteConsole",
                            Input = new () {
                                Type = EfeuExpressionType.Script,
                                Code = "var"
                            }
                        },
                    ]
                }
            ];

            EfeuRuntimeSimulation simulation = EfeuRuntimeSimulation.Run(steps);
            simulation.Send(new EfeuMessage()
            {
                Id = Guid.NewGuid(),
                Type = "A",
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
