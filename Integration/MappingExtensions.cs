using System.Collections.Immutable;
using Efeu.Integration.Utils.Serialization;

namespace Efeu.Integration;

using Efeu.Integration.Entities;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MappingExtensions
{
    public static TriggerEntity MapToTriggerEntity(this EfeuTrigger model, EfeuValueSerializer serializer)
    {
        return new TriggerEntity()
        {
            Id = model.Id,
            BehaviourVersionId = model.BehaviourId,
            CorrelationId = model.CorrelationId,
            CreationTime = model.CreationTime,
            Group = model.Group,
            Input = serializer.Serialize(model.Input),
            Matter = model.Matter,
            Position = model.Position,
            Scope = serializer.Serialize(new EfeuHash(model.Scope.Constants)),
            LoopbackScope = model.Scope.Loopback == null ? 
                "" : serializer.Serialize(new EfeuHash(model.Scope.Loopback.Scope.Constants)),
            LoopbackPosition = model.Scope.Loopback?.Position ?? "",
            Tag = model.Tag,
            Type = model.Type
        };
    }

    public static EfeuTrigger MapToEfeuTrigger(this TriggerEntity entity, EfeuBehaviourStep step, EfeuValue inputValue, EfeuValue scopeValue)
    {
        EfeuRuntimeScope runtimeScope = new EfeuRuntimeScope(scopeValue.AsHash().Hash);
        return new EfeuTrigger()
        {
            Id = entity.Id,
            CorrelationId = entity.CorrelationId,
            CreationTime = entity.CreationTime,
            Group = entity.Group,
            Input = inputValue,
            Matter = entity.Matter,
            Position = entity.Position,
            Scope = runtimeScope,
            Tag = entity.Tag,
            Step = step,
            Type = entity.Type
        };
    }

    public static EfeuTrigger MapToEfeuTrigger(this TriggerEntity entity, EfeuBehaviourStep step, EfeuValue inputValue, EfeuValue scopeValue, EfeuBehaviourStep loopbackStep, EfeuValue loopbackScopeValue)
    {
        EfeuRuntimeLoopback loopback = new EfeuRuntimeLoopback(entity.LoopbackPosition, loopbackStep, new EfeuRuntimeScope(loopbackScopeValue.AsHash().Hash));
        EfeuRuntimeScope runtimeScope = new EfeuRuntimeScope(scopeValue.AsHash().Hash, loopback);
        
        return new EfeuTrigger()
        {
            Id = entity.Id,
            CorrelationId = entity.CorrelationId,
            CreationTime = entity.CreationTime,
            Group = entity.Group,
            Input = inputValue,
            Matter = entity.Matter,
            Position = entity.Position,
            Scope = runtimeScope,
            Tag = entity.Tag,
            Step = step,
            Type = entity.Type
        };
    }

    public static EfeuMessage MapToEfeuMessage(this EffectEntity entity)
    {
        return new EfeuMessage()
        {
            Id = entity.Id,
            Tag = entity.Tag,
            Type = entity.Type,
            Payload = entity.Data,
            Timestamp = entity.CreationTime,
            Matter = entity.Matter,
            CorrelationId = entity.CorrelationId,
        };
    }

    public static EffectEntity MapToEffectEntity(this EfeuMessage model, EfeuValueSerializer serializer)
    {
        return new EffectEntity()
        {
            Id = model.Id,
            Type = model.Type,
            Tag = model.Tag,
            Input = serializer.Serialize(model.Payload),
            CorrelationId = model.CorrelationId,
            CreationTime = model.Timestamp,
            Matter = model.Matter,
        };
    }
}
