using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Efeu.Router.Value;
using Efeu.Router.Json.Converters;
using Efeu.Router.Script;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Efeu.Router
{
    internal class Program
    {
        static void Main(string[] args)
        {
            EfeuScriptScope scope = EfeuScriptScope.Empty
                .Push("name", 2)
                .Push("a", 2);

            string script = "a";
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            EfeuValue value = EfeuScript.Run(script, scope);
            Console.WriteLine(JsonSerializer.Serialize(value, options));

            BehaviourDefinitionStep[] steps = [
                new () {
                    Type = BehaviourStepType.Let,
                    Name = "Value",
                    Input = EfeuExpression.Eval(1),
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
                            Input = EfeuExpression.Eval(1),
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

            BehaviourRuntime behaviour1 = BehaviourRuntime.Run(steps, Guid.NewGuid(), 10);
            BehaviourTrigger trigger = behaviour1.Triggers.First();

            BehaviourRuntime behaviour2 = BehaviourRuntime.RunTrigger(trigger, new EfeuMessage()
            {
                 Tag = EfeuMessageTag.Incoming,
                 Name = "Event"
            });

            Console.WriteLine("done");
        }
    }
}
