using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class CachedLookup<TKey, TValue> where TKey : notnull
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

        public void Inject(TKey key, TValue value)
        {
            cache[key] = value;
        }
    }

    internal class TriggerProcessingContext
    {
        public readonly HashSet<Guid> ResolvedMatters = [];
        public readonly HashSet<Guid> CompletedGroups = [];
        public readonly HashSet<EfeuTrigger> CreatedTriggers = [];

        private readonly List<TriggerEntity> triggerEntities = [];

        private readonly CachedLookup<int, BehaviourVersionEntity> behaviourVersionEntityCache;
        private readonly CachedLookup<Guid, BehaviourScopeEntity> behaviourScopeEntityCache;

        public TriggerProcessingContext(TriggerEntity[] triggerEntities, IBehaviourQueries behaviourQueries, IBehaviourScopeQueries behaviourScopeQueries, EfeuTrigger[] createdTriggers)
        {
            this.triggerEntities = triggerEntities.ToList();

            TriggerEntity[] createdTriggerEntities = createdTriggers.Select(i => i.MapToTriggerEntity()).ToArray();
            BehaviourScopeEntity[] createdBehaviourScopeEntities = createdTriggers.Select(i => i.Scope.MapToBehaviourScopeEntity(0)).ToArray();

            this.behaviourVersionEntityCache = new CachedLookup<int, BehaviourVersionEntity>(behaviourQueries.GetVersionsByIdsAsync, i => i.Id);
            this.behaviourScopeEntityCache = new CachedLookup<Guid, BehaviourScopeEntity>(createdBehaviourScopeEntities, behaviourScopeQueries.GetByIdsAsync, i => i.Id);

            foreach (EfeuTrigger trigger in createdTriggers)
            {
                CreatedTriggers.Add(trigger);
                this.triggerEntities.Add(trigger.MapToTriggerEntity());
            }
        }

        public void Apply(EfeuRuntime runtime)
        {
            if (runtime.Matter != Guid.Empty)
            {
                triggerEntities.RemoveAll(i => i.Matter == runtime.Matter);
                CreatedTriggers.RemoveAll(i => i.Matter == runtime.Matter);
                ResolvedMatters.Add(runtime.Matter);
            }

            if (runtime.Skipped)
                return;

            if (runtime.Group != Guid.Empty)
            {
                CreatedTriggers.RemoveAll(i => i.Group == runtime.Group);
                triggerEntities.RemoveAll(i => i.Group == runtime.Group);
                CompletedGroups.Add(runtime.Group);
            }

            foreach (EfeuTrigger trigger in runtime.Triggers)
            {
                CreatedTriggers.Add(trigger);
                triggerEntities.Add(trigger.MapToTriggerEntity());
                behaviourScopeEntityCache.Inject(trigger.Id, trigger.Scope.MapToBehaviourScopeEntity(0));
            }
        }

        public async Task<EfeuTrigger[]> GetMatchingTriggersAsync(EfeuMessage message)
        {
            TriggerEntity[] matchingTriggerEntities = triggerEntities.Where(i =>
                    i.Type == message.Type &&
                    i.Tag == message.Tag &&
                    i.Matter == message.Matter &&
                    i.CreationTime <= message.Timestamp)
                    .ToArray();

            await behaviourScopeEntityCache.GetAsync(triggerEntities.Select(i => i.ScopeId).ToArray());
            await behaviourVersionEntityCache.GetAsync(triggerEntities.Select(i => i.BehaviourVersionId).ToArray());
            return matchingTriggerEntities.Select(triggerEntity => triggerEntity.MapToEfeuTrigger(
                behaviourVersionEntityCache.GetCached(triggerEntity.BehaviourVersionId).GetPosition(triggerEntity.Position),
                behaviourScopeEntityCache.GetCached(triggerEntity.ScopeId).MapToEfeuRuntimeScope())).ToArray();
        }
    }
}
