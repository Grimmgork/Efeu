using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Simulator
{
    public class EfeuRuntimeSimulation
    {
        public IReadOnlyCollection<EfeuMessage> Messages => messages;
        public IReadOnlyCollection<EfeuTrigger> Triggers => triggers;


        private List<EfeuMessage> messages = [];
        private List<EfeuTrigger> triggers = [];

        private EfeuRuntimeSimulation()
        {

        }

        public static EfeuRuntimeSimulation Run(EfeuBehaviourStep[] steps)
        {
            EfeuRuntimeSimulation result = new EfeuRuntimeSimulation();
            EfeuRuntime runtime = EfeuRuntime.Run(steps, 0);
            result.messages.AddRange(runtime.Messages);
            result.triggers.AddRange(runtime.Triggers);

            foreach (EfeuMessage message in runtime.Messages)
            {
                result.messages.Add(message);
            }

            return result;
        }

        public void SendMessage(EfeuMessage message)
        {
            EfeuRuntime? runtime = null;
            EfeuTrigger[] matchingTriggers = GetMatchingTriggers(message);
            foreach (EfeuTrigger trigger in matchingTriggers)
            {
                runtime = EfeuRuntime.RunTrigger(trigger, message);
                if (runtime.Result == EfeuRuntimeResult.Executed)
                {
                    messages.AddRange(runtime?.Messages ?? []);
                    triggers.AddRange(runtime?.Triggers ?? []);
                    if (!trigger.IsStatic)
                    {
                        triggers.Remove(trigger);
                    }
                }
            }
        }

        public void SendMessages(params EfeuMessage[] messages)
        {
            foreach (EfeuMessage message in messages)
            {
                SendMessage(message);
            }
        }

        private EfeuTrigger[] GetMatchingTriggers(EfeuMessage message)
        {
            return triggers.Where(trigger =>
                message.Tag == trigger.Tag &&
                message.Type == trigger.Type &&
                message.Matter == trigger.Matter).ToArray();
        }
    }
}
