using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class EfeuRuntimeSimulation
    {
        public List<EfeuMessage> Messages = new List<EfeuMessage>();
        public List<EfeuTrigger> Triggers = new List<EfeuTrigger>();

        private EfeuRuntimeSimulation() { }

        public static EfeuRuntimeSimulation Run(EfeuBehaviourStep[] steps, int definitionId = 0)
        {
            EfeuRuntimeSimulation simulation = new EfeuRuntimeSimulation();
            EfeuRuntime runtime = EfeuRuntime.Run(steps, definitionId, DateTime.Now);

            simulation.Messages.AddRange(runtime.Messages);
            simulation.Triggers.AddRange(runtime.Triggers);

            foreach (EfeuMessage message in runtime.Messages)
            {
                if (message.Tag != EfeuMessageTag.Effect)
                {
                    simulation.Send(message);
                }
            }

            return simulation;
        }

        public void Send(EfeuMessage message)
        {
            EfeuTrigger[] matchingTriggers = GetMatchingTriggers(message);
            foreach (EfeuTrigger trigger in matchingTriggers)
            {
                EfeuRuntime runtime = EfeuRuntime.RunTrigger(trigger, message);
                Apply(runtime);
            }
        }

        private void Apply(EfeuRuntime runtime)
        {
            if (runtime.Matter != Guid.Empty)
            {
                Triggers.RemoveAll(i => i.Matter == runtime.Matter);
            }

            if (runtime.Skipped)
                return;

            if (runtime.Group != Guid.Empty)
            {
                Triggers.RemoveAll(i => i.Group == runtime.Group);
            }

            Messages.AddRange(runtime.Messages);
            foreach (EfeuMessage message in runtime.Messages)
                if (message.Tag != EfeuMessageTag.Effect)
                    Send(message);
        }

        private EfeuTrigger[] GetMatchingTriggers(EfeuMessage message)
        {
            return Triggers.ToArray();
        }
    }
}
