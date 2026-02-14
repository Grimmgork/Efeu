using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class EfeuRuntimeSimulation
    {
        public IReadOnlyCollection<EfeuMessage> Messages => messages;
        public IReadOnlyCollection<EfeuTrigger> Triggers => triggers;

        private List<EfeuMessage> messages = [];
        private List<EfeuTrigger> triggers = [];

        private EfeuRuntimeSimulation() { }

        public static EfeuRuntimeSimulation Run(EfeuBehaviourStep[] steps, int definitionId = 0)
        {
            EfeuRuntimeSimulation result = new EfeuRuntimeSimulation();
            EfeuRuntime runtime = EfeuRuntime.Run(steps, definitionId, DateTime.Now);
            result.messages.AddRange(runtime.Messages);
            result.triggers.AddRange(runtime.Triggers);
            return result;
        }

        public void SendMessage(EfeuMessage message)
        {
            EfeuRuntime? runtime = null;
            List<EfeuMessage> messages = new List<EfeuMessage>();
            List<EfeuTrigger> triggers = new List<EfeuTrigger>();
            List<EfeuTrigger> removedTriggers = new List<EfeuTrigger>();

            foreach (EfeuTrigger trigger in this.triggers)
            {
                runtime = EfeuRuntime.RunTrigger(trigger, message);
                if (runtime.Result == EfeuRuntimeResult.Executed)
                {
                    messages.AddRange(runtime?.Messages ?? []);
                    triggers.AddRange(runtime?.Triggers ?? []);
                    if (!trigger.IsStatic)
                    {
                        removedTriggers.Add(trigger);
                    }
                }
            }

            // apply changes
            this.messages.AddRange(messages);
            this.triggers.AddRange(triggers);

            foreach (EfeuTrigger removedTrigger in removedTriggers)
                this.triggers.Remove(removedTrigger);
        }
    }
}
