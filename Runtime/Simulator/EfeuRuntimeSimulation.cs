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

        public EfeuRuntimeSimulation(EfeuBehaviourStep[] steps)
        {
            this.steps = steps;
        }

        public void Run(EfeuBehaviourStep[] steps)
        {
            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), 10);
            Messages.AddRange(runtime.Messages);
            Triggers.AddRange(runtime.Triggers);
            
            foreach (EfeuMessage message in runtime.Messages)
            {
                if (message.Tag == EfeuMessageTag.Effect)
                {

                }
                else
                {
                    SendMessage(message);
                }
            }
        }

        public void SendMessage(EfeuMessage message)
        {
            // find matching triggers
            // run matching triggers
            // while 

            EfeuRuntime runtime = EfeuRuntime.Run(steps, Guid.NewGuid(), 10);
            Messages.AddRange(runtime.Messages);
            Triggers.AddRange(runtime.Triggers);
        }
    }
}
