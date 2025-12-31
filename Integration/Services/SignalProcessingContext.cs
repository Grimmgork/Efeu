using Efeu.Integration.Commands;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Services
{
    public class SignalProcessingContext
    {
        public readonly DateTimeOffset Timestamp;
        public readonly List<BehaviourTrigger> Triggers = new List<BehaviourTrigger>();
        public readonly Stack<EfeuMessage> Messages = new Stack<EfeuMessage>();

        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;

        public readonly HashSet<Guid> DeletedTriggers = new();

        public SignalProcessingContext(IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, DateTimeOffset timestamp)
        {
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            Timestamp = timestamp;
        }

        private readonly Dictionary<int, BehaviourDefinitionVersionEntity> definitionEntityCache = new();

        public async Task ProcessSignalAsync(EfeuMessage message)
        {
            BehaviourTrigger[] matchingTriggers = await GetMatchingTriggersAsync(message.Name, message.Tag, message.TriggerId);
            foreach (BehaviourTrigger trigger in matchingTriggers)
            {
                BehaviourRuntime runtime;
                if (trigger.IsStatic)
                {
                    runtime = BehaviourRuntime.RunStaticTrigger(trigger, message, Guid.NewGuid());
                }
                else
                {
                    runtime = BehaviourRuntime.RunTrigger(trigger, message);
                }

                if (runtime.Result == BehaviourRuntimeResult.Skipped)
                    continue;

                if (!trigger.IsStatic)
                {
                    Triggers.RemoveAll(item => item.Id == trigger.Id);
                    DeletedTriggers.Add(trigger.Id);
                }

                foreach (BehaviourTrigger trigger1 in runtime.Triggers)
                {
                    Triggers.Add(trigger1);
                }

                foreach (EfeuMessage outMessage in runtime.Messages)
                {
                    Messages.Push(outMessage);
                }
            }
        }

        private async Task<BehaviourTrigger[]> GetMatchingTriggersAsync(string messageName, EfeuMessageTag messageTag, Guid triggerId)
        {
            BehaviourTriggerEntity[] triggerEntities = await behaviourTriggerRepository.GetMatchingAsync(messageName, messageTag, triggerId);

            BehaviourDefinitionVersionEntity[] definitionEntities = await behaviourDefinitionRepository.GetVersionsByIdsAsync(
                triggerEntities.Select(i => i.DefinitionVersionId)
                    .Where(i => !definitionEntityCache.ContainsKey(i)).ToArray());

            foreach (BehaviourDefinitionVersionEntity definitionVersionEntity in definitionEntities)
                definitionEntityCache.Add(definitionVersionEntity.Id, definitionVersionEntity);

            List<BehaviourTrigger> result = new();
            foreach (BehaviourTriggerEntity triggerEntity in triggerEntities)
            {
                if (DeletedTriggers.Contains(triggerEntity.Id))
                    continue;

                BehaviourTrigger trigger = new BehaviourTrigger()
                {
                    Id = triggerEntity.Id,
                    CorrelationId = triggerEntity.CorrelationId,
                    MessageName = triggerEntity.MessageName,
                    MessageTag = triggerEntity.MessageTag,
                    Scope = triggerEntity.Scope,
                    Position = triggerEntity.Position,
                    DefinitionId = triggerEntity.DefinitionVersionId,
                    Step = definitionEntityCache[triggerEntity.DefinitionVersionId].GetPosition(triggerEntity.Position)
                };
                result.Add(trigger);
            }

            result.AddRange(
                Triggers.Where(i =>
                    i.MessageName == messageName &&
                    i.MessageTag == messageTag &&
                    (triggerId == Guid.Empty ? true : i.Id == triggerId)));

            return result.ToArray();
        }
    }
}
