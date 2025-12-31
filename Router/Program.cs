using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Efeu.Router.Data;
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
            string script =
                "[1,2 ,3 ]";

            EfeuScriptScope scope = new EfeuScriptScope();
            scope.Assign("A", 10);

            Console.WriteLine(EfeuScript.Run(script, scope));
            return;


            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            BehaviourDefinitionStep[] steps = [
                new () {
                    Type = BehaviourStepType.Let,
                    Name = "Value",
                    Expression = (context) => false
                },
                new () {
                    Type = BehaviourStepType.Emit,
                    Name = "HelloWorld"
                },
                new () {
                    Type = BehaviourStepType.Await,
                    Name = "Event",
                    Where = [
                        new () {
                            Field = "Name",
                            Script = "1 + 2"
                        },
                        new () {
                            Field = "Name",
                            Literal = 1
                        }
                    ],
                    Do = [
                        new () {
                            Type = BehaviourStepType.If,
                            Expression = (context) => context.Constant("Value"),
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
