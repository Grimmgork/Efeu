using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Efeu.Integration.Utils;

namespace Efeu.Integration.Commands;

internal class TriggerProcessingContext
{
    public readonly HashSet<Guid> ResolvedMatters = [];
    public readonly HashSet<Guid> CompletedGroups = [];
    public readonly HashSet<EfeuTrigger> CreatedTriggers = [];

    private readonly List<TriggerEntity> triggerEntities = [];

    private readonly CachedLookup<Guid, BehaviourVersionEntity> behaviourVersionEntityCache;
    private readonly CachedLookup<Guid, BehaviourScopeEntity> behaviourScopeEntityCache;

    public TriggerProcessingContext(TriggerEntity[] triggerEntities, IBehaviourQueries behaviourQueries, IBehaviourScopeQueries behaviourScopeQueries, EfeuTrigger[] createdTriggers)
    {
        this.triggerEntities = triggerEntities.ToList();

        TriggerEntity[] createdTriggerEntities = createdTriggers.Select(i => i.MapToTriggerEntity()).ToArray();
        BehaviourScopeEntity[] createdBehaviourScopeEntities = createdTriggers.Select(i => i.Scope.MapToBehaviourScopeEntity(0)).ToArray();

        this.behaviourVersionEntityCache = new CachedLookup<Guid, BehaviourVersionEntity>(behaviourQueries.GetVersionsByIdsAsync, i => i.Id);
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

        List<EfeuTrigger> result = new List<EfeuTrigger>();
        foreach (TriggerEntity triggerEntity in matchingTriggerEntities)
        {
            BehaviourVersionEntity behaviourVersionEntity = behaviourVersionEntityCache.GetCached(triggerEntity.BehaviourVersionId);
            EfeuBehaviourStep behaviourStep = behaviourVersionEntity.GetPosition(triggerEntity.Position);

            EfeuRuntimeScope runtimeScope = GetScopeFromCache(triggerEntity.ScopeId, behaviourVersionEntity);
            result.Add(triggerEntity.MapToEfeuTrigger(behaviourStep, runtimeScope));
        }
        return result.ToArray();
    }

    private EfeuRuntimeScope GetScopeFromCache(Guid scopeId, BehaviourVersionEntity behaviourVersionEntity)
    {
        BehaviourScopeEntity scopeEntity = behaviourScopeEntityCache.GetCached(scopeId);
        if (scopeEntity.LoopbackScopeId == Guid.Empty)
        {
            return scopeEntity.MapToEfeuRuntimeScope();
        }
        else
        {
            EfeuRuntimeScope loopbackScope = GetScopeFromCache(scopeEntity.LoopbackScopeId, behaviourVersionEntity);
            EfeuBehaviourStep loopbackStep = behaviourVersionEntity.GetPosition(scopeEntity.LoopbackPosition);
            return scopeEntity.MapToEfeuRuntimeScope(loopbackStep, loopbackScope);
        }
    }
}
