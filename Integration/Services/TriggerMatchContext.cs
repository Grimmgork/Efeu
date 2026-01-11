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
    public class TriggerMatchContext
    {
        public readonly DateTimeOffset Timestamp;
        public readonly List<EfeuTrigger> Triggers = new List<EfeuTrigger>();
        public readonly Stack<EfeuMessage> Messages = new Stack<EfeuMessage>();

        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;

        public readonly HashSet<Guid> DeletedTriggers = new();

        public TriggerMatchContext(IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, DateTimeOffset timestamp)
        {
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            Timestamp = timestamp;
        }

        private readonly Dictionary<int, BehaviourDefinitionVersionEntity> definitionEntityCache = new();

        public async Task MatchTriggersAsync(EfeuMessage signal)
        {
            EfeuTrigger[] matchingTriggers = await GetMatchingTriggersAsync(signal.Name, signal.Tag, signal.TriggerId);
            foreach (EfeuTrigger trigger in matchingTriggers)
            {
                EfeuRuntime runtime;
                if (trigger.IsStatic)
                {
                    runtime = EfeuRuntime.RunStaticTrigger(trigger, signal, Guid.NewGuid());
                }
                else
                {
                    runtime = EfeuRuntime.RunTrigger(trigger, signal);
                }

                if (runtime.Result == EfeuRuntimeResult.Skipped)
                    continue;

                if (!trigger.IsStatic)
                {
                    Triggers.RemoveAll(item => item.Id == trigger.Id);
                    DeletedTriggers.Add(trigger.Id);
                }

                foreach (EfeuTrigger trigger1 in runtime.Triggers)
                {
                    Triggers.Add(trigger1);
                }

                foreach (EfeuMessage outMessage in runtime.Messages)
                {
                    Messages.Push(outMessage);
                }
            }
        }

        private async Task<EfeuTrigger[]> GetMatchingTriggersAsync(string messageName, EfeuMessageTag messageTag, Guid triggerId)
        {
            BehaviourTriggerEntity[] triggerEntities = await behaviourTriggerRepository.GetMatchingAsync(messageName, messageTag, triggerId, Timestamp);

            BehaviourDefinitionVersionEntity[] definitionEntities = await behaviourDefinitionRepository.GetVersionsByIdsAsync(
                triggerEntities.Select(i => i.DefinitionVersionId)
                    .Where(i => !definitionEntityCache.ContainsKey(i)).ToArray());

            foreach (BehaviourDefinitionVersionEntity definitionVersionEntity in definitionEntities)
                definitionEntityCache.Add(definitionVersionEntity.Id, definitionVersionEntity);

            List<EfeuTrigger> result = new();
            foreach (BehaviourTriggerEntity triggerEntity in triggerEntities)
            {
                if (DeletedTriggers.Contains(triggerEntity.Id))
                    continue;

                EfeuTrigger trigger = new EfeuTrigger()
                {
                    Id = triggerEntity.Id,
                    CorrelationId = triggerEntity.CorrelationId,
                    Name = triggerEntity.Name,
                    Tag = triggerEntity.Tag,
                    Scope = triggerEntity.Scope,
                    Position = triggerEntity.Position,
                    DefinitionId = triggerEntity.DefinitionVersionId,
                    Step = definitionEntityCache[triggerEntity.DefinitionVersionId].GetPosition(triggerEntity.Position)
                };
                result.Add(trigger);
            }

            result.AddRange(
                Triggers.Where(i =>
                    i.Name == messageName &&
                    i.Tag == messageTag &&
                    (triggerId == Guid.Empty ? true : i.Id == triggerId)));

            return result.ToArray();
        }
    }
}
