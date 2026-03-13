namespace Efeu.Integration
{
    using Efeu.Integration.Entities;
    using Efeu.Runtime;

    public static class MappingExtensions
    {
        public static TriggerEntity MapToEntity(this EfeuTrigger model)
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
                Scope = model.Scope,
                Tag = model.Tag,
                Type = model.Type
            };
        }

        public static EfeuTrigger MapToModel(this TriggerEntity entity, EfeuBehaviourStep step)
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
                Scope = entity.Scope,
                Tag = entity.Tag,
                Step = step,
                Type = entity.Type
            };
        }

        public static PartialTriggerEntity MapToPartialEntity(this EfeuTrigger model)
        {
            return new PartialTriggerEntity()
            {
                Id = model.Id,
                CreationTime = model.CreationTime,
                Group = model.Group,
                Matter = model.Matter,
                Tag = model.Tag,
                Type = model.Type,
            };
        }

        public static EfeuMessage MapToMessage(this EffectEntity entity)
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

    }
}
