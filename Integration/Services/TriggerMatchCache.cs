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
        public readonly List<EfeuTrigger> Triggers = new List<EfeuTrigger>();
        public readonly Stack<EfeuMessage> Messages = new Stack<EfeuMessage>();

        private readonly ITriggerQueries triggerQueries;
        private readonly IBehaviourQueries behaviourQueries;

        public readonly HashSet<Guid> DeletedTriggers = new();
        public readonly HashSet<Guid> ResolvedMatters = new();

        public TriggerMatchCache(ITriggerQueries triggerQueries, IBehaviourQueries behaviourQueries)
        {
            this.behaviourQueries = behaviourQueries;
            this.triggerQueries = triggerQueries;
        }

        private readonly Dictionary<int, BehaviourVersionEntity> behaviourVersionEntityCache = new();

        public async Task MatchTriggersAsync(EfeuMessage message)
        {
            EfeuTrigger[] matchingTriggers = await GetMatchingTriggersAsync(message);
            foreach (EfeuTrigger trigger in matchingTriggers)
            {
                EfeuRuntime runtime = EfeuRuntime.RunTrigger(trigger, message);

                if (runtime.IsSkipped)
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

        private async Task<EfeuTrigger[]> GetMatchingTriggersAsync(EfeuMessage message)
        {
            TriggerEntity[] triggerEntities = await triggerQueries.GetMatchingAsync(message.Type, message.Tag, message.Matter, message.Timestamp);

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
                    i.Type == message.Type &&
                    i.Tag == message.Tag &&
                    i.Matter == message.Matter &&
                    i.CreationTime <= message.Timestamp));

            return result.ToArray();
        }
    }
}
