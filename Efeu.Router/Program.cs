using Efeu.Runtime.JSON.Converters;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Efeu.Router
{
    internal class Program
    {
        static void Main(string[] args)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new EfeuValueJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            BehaviourDefinition definition = new BehaviourDefinition()
            {
                Id = "definition1",
                Steps = [
                    new () {
                        Type = BehaviourStepType.Let,
                        Name = "Value",
                        Input = (context) => false
                    },
                    new () {
                        Type = BehaviourStepType.Emit,
                        Name = "HelloWorld"
                    },
                    new () {
                        Type = BehaviourStepType.Await,
                        Name = "ConsoleInput",
                        Do = [
                            new () {
                                Type = BehaviourStepType.If,
                                Input = (context) => context.Constant("Value"),
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
                ]
            };
            
            BehaviourRuntime behaviour1 = BehaviourRuntime.Run(definition, Guid.NewGuid().ToString());
            BehaviourTrigger trigger = behaviour1.Triggers.First();

            BehaviourRuntime behaviour2 = BehaviourRuntime.RunTrigger(definition, trigger, new EfeuMessage()
            {
                 Tag = EfeuMessageTag.Data,
                 Name = "ConsoleInput",
                 InstanceId = trigger.InstanceId,
            });

            Console.WriteLine("done");
        }
    }
}
