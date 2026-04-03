namespace Efeu.Integration
{
    using Efeu.Integration.Entities;
    using Efeu.Runtime;
    using System.Collections.Generic;
    using System.Linq;

    public static class MappingExtensions
    {
        public static TriggerEntity MapToTriggerEntity(this EfeuTrigger model)
        {
            return new TriggerEntity()
            {
                Id = model.Id,
                BehaviourVersionId = model.BehaviourId,
                CorrelationId = model.CorrelationId,
                CreationTime = model.CreationTime,
                Group = model.Group,
                Input = model.Input,
                Matter = model.Matter,
                Position = model.Position,
                ScopeId = model.Scope.Id,
                Tag = model.Tag,
                Type = model.Type
            };
        }

        public static EfeuTrigger MapToEfeuTrigger(this TriggerEntity entity, EfeuBehaviourStep step, EfeuRuntimeScope scope)
        {
            return new EfeuTrigger()
            {
                Id = entity.Id,
                CorrelationId = entity.CorrelationId,
                CreationTime = entity.CreationTime,
                Group = entity.Group,
                Input = entity.Input,
                Matter = entity.Matter,
                Position = entity.Position,
                Scope = scope,
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

        public static EffectEntity MapToEffectEntity(this EfeuMessage model)
        {
            return new EffectEntity()
            {
                Id = model.Id,
                Type = model.Type,
                Tag = model.Tag,
                Input = model.Payload,
                CorrelationId = model.CorrelationId,
                CreationTime = model.Timestamp,
                Matter = model.Matter,
            };
        }

        public static BehaviourScopeEntity MapToBehaviourScopeEntity(this EfeuRuntimeScope scope, uint referenceCount)
        {
            return new BehaviourScopeEntity()
            {
                Id = scope.Id,
                Constants = scope.Constants,
                ReferenceCount = referenceCount
            };
        }

        public static EfeuRuntimeScope MapToEfeuRuntimeScope(this BehaviourScopeEntity scopeEntity)
        {
            return new EfeuRuntimeScope(scopeEntity.Id, scopeEntity.Constants);
        }


        public static BehaviourScopeEntity[] MapToBehaviourScopeEntities(this IEnumerable<EfeuTrigger> triggers)
        {
            HashSet<EfeuRuntimeScope> triggerScopes = new HashSet<EfeuRuntimeScope>();
            Dictionary<EfeuRuntimeScope, uint> triggerScopeReferenceCounts = new Dictionary<EfeuRuntimeScope, uint>();
            foreach (EfeuTrigger trigger in triggers)
            {
                if (triggerScopes.Add(trigger.Scope))
                {
                    triggerScopeReferenceCounts.Add(trigger.Scope, 1);
                }
                else
                {
                    triggerScopeReferenceCounts[trigger.Scope]++;
                }
            }

            return triggerScopes.Select(i => i.MapToBehaviourScopeEntity(triggerScopeReferenceCounts[i])).ToArray();
        }
    }
}
