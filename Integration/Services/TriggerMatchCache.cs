using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Efeu.Integration.Services
{
    public class TriggerMatchCache
    {
        public readonly DateTimeOffset Timestamp;
        public readonly List<EfeuTrigger> Triggers = new List<EfeuTrigger>();
        public readonly Stack<EfeuMessage> Messages = new Stack<EfeuMessage>();

        private readonly ITriggerQueries triggerQueries;
        private readonly IBehaviourQueries behaviourQueries;

        public readonly HashSet<Guid> DeletedTriggers = new();
        public readonly HashSet<Guid> ResolvedMatters = new();

        public TriggerMatchCache(ITriggerQueries triggerQueries, IBehaviourQueries behaviourQueries, DateTimeOffset timestamp)
        {
            this.behaviourQueries = behaviourQueries;
            this.triggerQueries = triggerQueries;
            Timestamp = timestamp;
        }

        private readonly Dictionary<int, BehaviourVersionEntity> behaviourVersionEntityCache = new();

        public async Task MatchTriggersAsync(EfeuMessage signal)
        {
            EfeuTrigger[] matchingTriggers = await GetMatchingTriggersAsync(signal.Type, signal.Tag, signal.Matter);
            foreach (EfeuTrigger trigger in matchingTriggers)
            {
                EfeuRuntime runtime = EfeuRuntime.RunTrigger(trigger, signal);

                if (runtime.Result == EfeuRuntimeResult.Skipped)
                    continue;

                if (!trigger.IsStatic)
                {
                    Triggers.RemoveAll(item => item.Id == trigger.Id);
                    DeletedTriggers.Add(trigger.Id);
                    if (trigger.Matter != Guid.Empty)
                    {
                        Triggers.RemoveAll(item => item.Matter == trigger.Matter);
                        ResolvedMatters.Add(trigger.Matter);
                    }
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

        private async Task<EfeuTrigger[]> GetMatchingTriggersAsync(string messageName, EfeuMessageTag messageTag, Guid messageMatter)
        {
            TriggerEntity[] triggerEntities = await triggerQueries.GetMatchingAsync(messageName, messageTag, messageMatter, Timestamp);

            BehaviourVersionEntity[] behaviourVersionEntities = await behaviourQueries.GetVersionsByIdsAsync(
                triggerEntities.Select(i => i.BehaviourVersionId)
                    .Where(i => !behaviourVersionEntityCache.ContainsKey(i)).ToArray());

            foreach (BehaviourVersionEntity behaviourVersionEntity in behaviourVersionEntities)
                behaviourVersionEntityCache.Add(behaviourVersionEntity.Id, behaviourVersionEntity);

            List<EfeuTrigger> result = new();
            foreach (TriggerEntity triggerEntity in triggerEntities)
            {
                if (DeletedTriggers.Contains(triggerEntity.Id))
                    continue;

                if (triggerEntity.Matter != Guid.Empty)
                    if (ResolvedMatters.Contains(triggerEntity.Matter))
                        continue;

                EfeuTrigger trigger = new EfeuTrigger()
                {
                    Id = triggerEntity.Id,
                    CorrelationId = triggerEntity.CorrelationId,
                    Type = triggerEntity.Type,
                    Tag = triggerEntity.Tag,
                    Scope = triggerEntity.Scope,
                    Position = triggerEntity.Position,
                    BehaviourId = triggerEntity.BehaviourVersionId,
                    Matter = triggerEntity.Matter,
                    Step = behaviourVersionEntityCache[triggerEntity.BehaviourVersionId].GetPosition(triggerEntity.Position)
                };
                result.Add(trigger);
            }

            result.AddRange(
                Triggers.Where(i =>
                    i.Type == messageName &&
                    i.Tag == messageTag &&
                    i.Matter == messageMatter));

            return result.ToArray();
        }
    }
}
