using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Efeu.Integration.Services
{

    public class CachedLookup<TKey, TValue> where TKey : notnull
    {
        private Dictionary<TKey, TValue> cache = new Dictionary<TKey, TValue>();

        private Func<TKey[], Task<TValue[]>> fetch;

        private Func<TValue, TKey> getKey;

        public CachedLookup(Func<TKey[], Task<TValue[]>> fetch, Func<TValue, TKey> getKey)
        {
            this.fetch = fetch;
            this.getKey = getKey;
        }

        public CachedLookup(TValue[] items, Func<TKey[], Task<TValue[]>> fetch, Func<TValue, TKey> getKey)
        {
            this.fetch = fetch;
            this.getKey = getKey;
            foreach (TValue item in items)
            {
                cache.Add(getKey(item), item);
            }
        }

        public async Task<TValue[]> GetAsync(TKey[] keys)
        {
            IEnumerable<TKey> missingKeys = keys.Where(i => !cache.ContainsKey(i));
            if (missingKeys.Any())
            {
                TValue[] missingValues = await fetch(missingKeys.ToArray());
                foreach (TValue value in missingValues)
                {
                    cache.Add(getKey(value), value);
                }
            }

            return keys.Select(i => cache[i]).ToArray();
        }

        public async Task<TValue> GetAsync(TKey key)
        {
            TValue[] result = await GetAsync([key]);
            return result.First();
        }

        public TValue GetCached(TKey key)
        {
            return cache[key];
        }
    }

    public class TriggerEntityCache
    {
        public readonly HashSet<Guid> ResolvedMatters = [];
        public readonly HashSet<Guid> CompletedGroups = [];
        public readonly List<EfeuTrigger> CreatedTriggers = [];

        private readonly List<PartialTriggerEntity> partialTriggerEntities = [];

        private readonly CachedLookup<Guid, TriggerEntity> triggerEntityCache;
        private readonly CachedLookup<int, BehaviourVersionEntity> behaviourVersionEntityCache;

        public TriggerEntityCache(PartialTriggerEntity[] triggerEntityKeys, ITriggerQueries triggerQueries, IBehaviourQueries behaviourQueries, EfeuTrigger[] createdTriggers)
        {
            this.partialTriggerEntities = triggerEntityKeys.ToList();

            TriggerEntity[] createdTriggerEntities = createdTriggers.Select(i => new TriggerEntity()
            {
                Id = i.Id,
                BehaviourVersionId = i.BehaviourId,
                CorrelationId = i.CorrelationId,
                CreationTime = i.CreationTime,
                Group = i.Group,
                Input = i.Input,
                Matter = i.Matter,
                Position = i.Position,
                Scope = i.Scope,
                Tag = i.Tag,
                Type = i.Type
            }).ToArray();

            this.triggerEntityCache = new CachedLookup<Guid, TriggerEntity>(createdTriggerEntities, triggerQueries.GetByIdsAsync, (i) => i.Id);
            this.behaviourVersionEntityCache = new CachedLookup<int, BehaviourVersionEntity>(behaviourQueries.GetVersionsByIdsAsync, (i) => i.Id);

            foreach (EfeuTrigger trigger in createdTriggers)
            {
                CreatedTriggers.Add(trigger);
                partialTriggerEntities.Add(new PartialTriggerEntity()
                {
                    Id = trigger.Id,
                    CreationTime = trigger.CreationTime,
                    Group = trigger.Group,
                    Matter = trigger.Matter,
                    Tag = trigger.Tag,
                    Type = trigger.Type,
                });
            }
        }

        public void Apply(EfeuRuntime runtime, EfeuMessage message, EfeuTrigger trigger)
        {
            if (message.Matter != Guid.Empty)
            {
                partialTriggerEntities.RemoveAll(i => i.Matter == message.Matter);
                CreatedTriggers.RemoveAll(i => i.Matter == message.Matter);
                ResolvedMatters.Add(message.Matter);
            }

            if (runtime.IsSkipped)
                return;

            if (!trigger.IsStatic)
            {
                CreatedTriggers.RemoveAll(i => i.Id == trigger.Id);
                partialTriggerEntities.RemoveAll(i => i.Id == trigger.Id);

                if (trigger.Group != Guid.Empty)
                {
                    CreatedTriggers.RemoveAll(i => i.Group == trigger.Group);
                    partialTriggerEntities.RemoveAll(i => i.Group == trigger.Group);
                    CompletedGroups.Add(trigger.Group);
                }
            }

            foreach (EfeuTrigger newTrigger in runtime.Triggers)
            {
                CreatedTriggers.Add(trigger);
                partialTriggerEntities.Add(new PartialTriggerEntity()
                {
                    Id = trigger.Id,
                    CreationTime = trigger.CreationTime,
                    Group = trigger.Group,
                    Matter = trigger.Matter,
                    Tag = trigger.Tag,
                    Type = trigger.Type,
                });
            }
        }

        public async Task<EfeuTrigger[]> GetMatchingTriggers(EfeuMessage message)
        {
            PartialTriggerEntity[] matchingKeys = partialTriggerEntities.Where(i =>
                    i.Type == message.Type &&
                    i.Tag == message.Tag &&
                    i.Matter == message.Matter &&
                    i.CreationTime <= message.Timestamp)
                    .ToArray();

            TriggerEntity[] triggerEntities = await triggerEntityCache.GetAsync(matchingKeys.Select(i => i.Id).ToArray());

            await behaviourVersionEntityCache.GetAsync(triggerEntities.Select(i => i.BehaviourVersionId).ToArray());
            return triggerEntities.Select(triggerEntity =>
                new EfeuTrigger()
                {
                    Id = triggerEntity.Id,
                    CorrelationId = triggerEntity.CorrelationId,
                    Type = triggerEntity.Type,
                    Tag = triggerEntity.Tag,
                    Scope = triggerEntity.Scope,
                    Position = triggerEntity.Position,
                    BehaviourId = triggerEntity.BehaviourVersionId,
                    Matter = triggerEntity.Matter,
                    Step = behaviourVersionEntityCache.GetCached(triggerEntity.BehaviourVersionId)
                                .GetPosition(triggerEntity.Position)
                }).ToArray();
        }
    }
}
