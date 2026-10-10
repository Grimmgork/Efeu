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
using Efeu.Integration.Utils.Serialization;

namespace Efeu.Integration.Commands;

internal class TriggerProcessingContext
{
    public readonly HashSet<Guid> ResolvedMatters = [];
    public readonly HashSet<Guid> CompletedGroups = [];
    public readonly HashSet<EfeuTrigger> CreatedTriggers;

    private readonly TriggerEntity[] triggerEntities;
    private readonly Dictionary<TriggerEntity, EfeuTrigger> triggerLookup = [];

    private readonly CachedLookup<Guid, BehaviourVersionEntity> behaviourVersionEntityCache;
    
    private readonly IValueNodeQueries valueNodeQueries;
    
    private readonly EfeuValueDeserializer efeuValueDeserializer;
    
    public TriggerProcessingContext(TriggerEntity[] triggerEntities, IBehaviourQueries behaviourQueries, IValueNodeQueries valueNodeQueries, EfeuTrigger[] createdTriggers)
    {
        this.triggerEntities = triggerEntities;
        
        this.behaviourVersionEntityCache = new CachedLookup<Guid, BehaviourVersionEntity>(behaviourQueries.GetVersionsByIdsAsync, i => i.Id);
        
        this.valueNodeQueries = valueNodeQueries;
        
        EfeuValueDeserializerOptions deserializerOptions = new()
        {
            Reader = new EfeuValueBinaryReader(),
        };
        
        this.efeuValueDeserializer = EfeuValueDeserializer.Begin(deserializerOptions);
        
        CreatedTriggers = new HashSet<EfeuTrigger>(createdTriggers);
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
        
        EfeuTrigger[] matchingCreatedTriggers = CreatedTriggers.Where(i =>
                i.Type == message.Type &&
                i.Tag == message.Tag &&
                i.Matter == message.Matter)
                .ToArray();
        
        EfeuTrigger[] result = await GetTriggersFromEntities(matchingTriggerEntities);
        return result.Concat(matchingCreatedTriggers).ToArray();
    }

    private async Task<EfeuTrigger[]> GetTriggersFromEntities(TriggerEntity[] entities)
    {
        var partition = entities.Partition(i => triggerLookup.ContainsKey(i));
        entities = partition.NonMatches.ToArray();
        
        int i = 0;
        string[] hashesToLoad = new string[entities.Length*3];
        foreach (TriggerEntity triggerEntity in entities)
        {
            hashesToLoad[i * 3 + 0] = triggerEntity.Input;
            hashesToLoad[i * 3 + 1] = triggerEntity.Scope;
            hashesToLoad[i * 3 + 2] = triggerEntity.LoopbackScope;
            i++;
        }
        
        EfeuValueSerializationResult serializationResult = await valueNodeQueries.ReadAsync(efeuValueDeserializer.GetNotDeserialized(hashesToLoad));
        efeuValueDeserializer.Deserialize(serializationResult);
        
        EfeuValue[] values = efeuValueDeserializer.Resolve(hashesToLoad.ToArray());
        
        await behaviourVersionEntityCache.GetAsync(triggerEntities.Select(i => i.BehaviourVersionId).ToArray());

        i = 0;
        EfeuTrigger[] result = new EfeuTrigger[entities.Length];
        foreach (TriggerEntity triggerEntity in entities)
        {
            BehaviourVersionEntity behaviourVersionEntity = behaviourVersionEntityCache.GetCached(triggerEntity.BehaviourVersionId);
            EfeuBehaviourStep behaviourStep = behaviourVersionEntity.GetPosition(triggerEntity.Position);

            EfeuTrigger trigger;
            if (string.IsNullOrEmpty(triggerEntity.LoopbackScope))
            {
                trigger = triggerEntity.MapToEfeuTrigger(behaviourStep, values[i*3], values[i*3+1]);
            }
            else
            {
                EfeuBehaviourStep loopbackBehaviourStep = behaviourVersionEntity.GetPosition(triggerEntity.LoopbackPosition);
                trigger = triggerEntity.MapToEfeuTrigger(behaviourStep, values[i*3], values[i*3+1], loopbackBehaviourStep, values[i*3+2]);
            }

            triggerLookup[triggerEntity] = trigger;
            result[i] = trigger;
            i++;
        }
        
        return result.Concat(partition.Matches.Select(e => triggerLookup[e])).ToArray();
    }
}
