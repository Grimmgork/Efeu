using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Simulator
{
    internal class EfeuRuntimeSimulation
    {
        public readonly List<EfeuMessage> Messages = new List<EfeuMessage>();

        public readonly List<EfeuTrigger> Triggers = new List<EfeuTrigger>();

        public void Run(EfeuBehaviourStep[] steps)
        {
            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), 10);
            Messages.AddRange(runtime.Messages);
            Triggers.AddRange(runtime.Triggers);
            
            foreach (EfeuMessage message in runtime.Messages)
            {
                Messages.Add(message);
            }
        }

        public void SendMessage(EfeuMessage message)
        {
            // find matching triggers
            // run matching triggers
            // while

            EfeuTrigger[] triggers = [];
            foreach (EfeuTrigger trigger in triggers)
            {
                if (trigger.IsStatic)
                {
                    EfeuRuntime runtime = EfeuRuntime.RunTrigger(trigger, message, Guid.NewGuid());
                }
                else
                {
                    EfeuRuntime runtime = EfeuRuntime.RunStaticTrigger(trigger, message, Guid.NewGuid());
                }
            }

            
            Messages.AddRange(runtime.Messages);
            Triggers.AddRange(runtime.Triggers);
        }

        private EfeuTrigger[] GetMatchingTriggers(EfeuMessage message)
        {

        }
    }
}
