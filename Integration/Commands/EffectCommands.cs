using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Foreign;
using Efeu.Integration.Persistence;
using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Integration.Utils.Serialization;

namespace Efeu.Integration.Commands;

internal class EffectCommands : IEffectCommands
{
    private readonly IEfeuUnitOfWork unitOfWork;
    private readonly IEffectQueries effectQueries;

    private readonly ITriggerCommands triggerCommands;
    private readonly ITriggerQueries triggerQueries;
    private readonly IBehaviourQueries behaviourQueries;
    private readonly IDeduplicationKeyCommands dedupicationKeyCommands;
    private readonly IValueNodeQueries valueNodeQueries;

    public EffectCommands(IEffectQueries effectQueries, IEfeuUnitOfWork unitOfWork, ITriggerCommands triggerCommands, ITriggerQueries triggerQueries, IBehaviourQueries behaviourQueries, IDeduplicationKeyCommands deduplicationKeyCommands, IValueNodeQueries valueNodeQueries)
    {
        this.effectQueries = effectQueries;
        this.unitOfWork = unitOfWork;
        this.triggerCommands = triggerCommands;
        this.triggerQueries = triggerQueries;
        this.behaviourQueries = behaviourQueries;
        this.dedupicationKeyCommands = deduplicationKeyCommands;
        this.valueNodeQueries = valueNodeQueries;
    }

    public async Task CreateEffect(EfeuMessage message)
    {
        await unitOfWork.BeginAsync();
        EfeuValueSerializerOptions serializerOptions = new ()
        {
            Hasher = new Sha256EfeuValueHasher(),
            Writer = new EfeuValueBinaryWriter()
        };
        
        EfeuValueSerializer efeuValueSerializer = EfeuValueSerializer.Begin(serializerOptions);

        EffectEntity effectEntity = message.MapToEffectEntity(efeuValueSerializer);
        await effectQueries.CreateAsync(effectEntity);

        EfeuValueSerializationResult serializationResult = efeuValueSerializer.End();
        await valueNodeQueries.WriteAsync(serializationResult);
        await unitOfWork.CompleteAsync();
    }

    public Task NudgeEffect(Guid id)
    {
        return effectQueries.NudgeEffectAsync(id);
    }

    public Task SuspendEffect(Guid id, DateTimeOffset timestamp)
    {
        return effectQueries.SuspendEffectAsync(id, timestamp);
    }

    public Task SkipEffect(Guid id, DateTimeOffset timestamp, EfeuValue output = default)
    {
        return effectQueries.CompleteSuspendedEffectAsync(id, timestamp, output);
    }

    public Task AbortEffect(Guid id)
    {
        return effectQueries.AbortEffectAsync(id);
    }

    public async Task RunImmediate(EfeuBehaviourStep[] steps, Guid definitionVersionId, DateTimeOffset timestamp)
    {
        await unitOfWork.BeginAsync();
        await unitOfWork.LockAsync("Trigger");
        EfeuRuntime runtime = EfeuRuntime.Run(steps, definitionVersionId, timestamp);
        await ProcessMessagesAsync(runtime.Messages.ToArray(), runtime.Triggers.ToArray());
        await unitOfWork.CompleteAsync();
    }

    public async Task SendMessageAsync(EfeuMessage message)
    {
        if (message.Id == Guid.Empty)
        {
            throw new Exception("message id is empty.");
        }

        if (message.Timestamp == DateTimeOffset.MinValue)
        {
            message.Timestamp = DateTime.Now;
        }

        if (message.CorrelationId == Guid.Empty)
        {
            message.CorrelationId = Guid.NewGuid();
        }

        await unitOfWork.BeginAsync();
        await unitOfWork.LockAsync("Trigger");

        if (!await dedupicationKeyCommands.TryInsertAsync(message.Id, message.Timestamp))
        {
            await unitOfWork.CompleteAsync();
            return;
        }

        await SendMessageDeduplicatedAsync(message);
        await unitOfWork.CompleteAsync();
    }

    public async Task SendMessageDeduplicatedAsync(EfeuMessage message)
    {
        if (message.Id == Guid.Empty)
        {
            message.Id = Guid.NewGuid();
        }

        if (message.Timestamp == DateTimeOffset.MinValue)
        {
            message.Timestamp = DateTime.Now;
        }

        if (message.CorrelationId == Guid.Empty)
        {
            message.CorrelationId = Guid.NewGuid();
        }

        await unitOfWork.BeginAsync();
        await unitOfWork.LockAsync("Trigger");
        if (message.Tag == EfeuMessageTag.Effect)
        {
            await CreateEffect(message);
        }
        else
        {
            await ProcessMessagesAsync([message], []);
        }

        await unitOfWork.CompleteAsync();
    }

    private async Task ProcessMessagesAsync(EfeuMessage[] messages, EfeuTrigger[] additionalTriggers)
    {
        TriggerEntity[] allTriggerEntities = await triggerQueries.GetAllAsync();
        TriggerProcessingContext context = new TriggerProcessingContext(allTriggerEntities, behaviourQueries, valueNodeQueries, additionalTriggers);

        int iterations = 0;
        Stack<EfeuMessage> messageStack = new Stack<EfeuMessage>(messages);
        List<EfeuMessage> effects = new List<EfeuMessage>();

        while (messageStack.TryPop(out EfeuMessage? message))
        {
            if (message.Tag == EfeuMessageTag.Effect)
            {
                effects.Add(message);
            }
            else
            {
                iterations++;
                if (iterations > 50)
                    throw new Exception($"infinite loop detected! ({iterations} iterations)");

                EfeuTrigger[] matchingTriggers = await context.GetMatchingTriggersAsync(message);
                foreach (EfeuTrigger trigger in matchingTriggers)
                {
                    EfeuRuntime runtime = EfeuRuntime.RunTrigger(trigger, message);
                    context.Apply(runtime);

                    foreach (EfeuMessage newMessage in runtime.Messages)
                        messageStack.Push(newMessage);
                }
            }
        }

        EfeuValueSerializerOptions serializerOptions = new ()
        {
            Hasher = new Sha256EfeuValueHasher(),
            Writer = new EfeuValueBinaryWriter()
        };
        
        EfeuValueSerializer efeuValueSerializer = EfeuValueSerializer.Begin(serializerOptions);
        
        int i = 0;
        TriggerEntity[] createdTriggers = new TriggerEntity[context.CreatedTriggers.Count];
        foreach (EfeuTrigger trigger in context.CreatedTriggers)
        {
            createdTriggers[i] = trigger.MapToTriggerEntity(efeuValueSerializer);
            i++;
        }

        i = 0;
        EffectEntity[] createdEffects = new EffectEntity[effects.Count];
        foreach (EfeuMessage message in effects)
        {
            createdEffects[i] = message.MapToEffectEntity(efeuValueSerializer);
            i++;
        }
        
        EfeuValueSerializationResult serializationResult = efeuValueSerializer.End();
        
        await triggerQueries.DetatchByMatterBulkAsync(context.ResolvedMatters.ToArray());
        await triggerQueries.DetatchByGroupBulkAsync(context.CompletedGroups.ToArray());
        await valueNodeQueries.WriteAsync(serializationResult);
        await triggerQueries.CreateBulkAsync(createdTriggers);
        await effectQueries.CreateBulkAsync(createdEffects);
    }
}
