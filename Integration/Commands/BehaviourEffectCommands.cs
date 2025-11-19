using Antlr4.Build.Tasks;
using Efeu.Integration.Entities;
using Efeu.Integration.Persistence;
using Efeu.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class BehaviourEffectCommands : IBehaviourEffectCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBehaviourEffectRepository behaviourEffectRepository;
        private readonly IBehaviourDefinitionRepository behaviourDefinitionRepository;
        private readonly IBehaviourDefinitionCommands behaviourDefinitionCommands;
        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;

        public BehaviourEffectCommands(IUnitOfWork unitOfWork, IBehaviourEffectRepository behaviourEffectRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, IBehaviourTriggerCommands behaviourTriggerCommands, IBehaviourTriggerRepository behaviourTriggerRepository, IBehaviourDefinitionCommands behaviourDefinitionCommands)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
            this.behaviourDefinitionCommands = behaviourDefinitionCommands;
        }

        public Task CreateEffect(EfeuMessage message)
        {
            return behaviourEffectRepository.CreateAsync(new BehaviourEffectEntity()
            {
                 CreationTime = DateTime.Now,
                 Name = message.Name,
                 TriggerId = message.TriggerId,
                 Data = message.Data,
                 CorrelationId = message.CorrelationId,
                 State = BehaviourEffectState.Running
            });
        }

        public async Task RunEffect(int id)
        {
            BehaviourEffectEntity effect = await behaviourEffectRepository.GetByIdAsync(id);

            // detect endcap
            if (effect.Name == "WriteConsole")
            {
                Console.WriteLine(effect.Data);
            }
            else
            {
                await SendSignal(new EfeuMessage()
                {
                    Name = effect.Name,
                    CorrelationId = effect.CorrelationId,
                    Data = effect.Data,
                    Tag = EfeuMessageTag.Effect,
                    TriggerId = effect.TriggerId,
                });
            }
        }

        private async Task SendSignal(EfeuMessage message)
        {
            BehaviourTriggerEntity[] triggerEntities = await behaviourTriggerRepository.GetMatchingAsync(message.Name, message.Tag);
            BehaviourDefinitionVersionEntity[] definitionEntities = await behaviourDefinitionRepository.GetVersionsByIdsAsync(triggerEntities.Select(i => i.DefinitionVersionId).ToArray());

            Dictionary<int, BehaviourDefinitionVersionEntity> definitions = definitionEntities.ToDictionary(i => i.Id);

            foreach (BehaviourTriggerEntity triggerEntity in triggerEntities)
            {
                BehaviourTrigger trigger = new BehaviourTrigger()
                {
                    Id = triggerEntity.Id,
                    CorrelationId = triggerEntity.CorrelationId,
                    MessageName = triggerEntity.MessageName,
                    MessageTag = triggerEntity.MessageTag,
                    Scope = triggerEntity.Scope,
                    Position = triggerEntity.Position,
                    Step = definitions[triggerEntity.DefinitionVersionId].GetPosition(triggerEntity.Position)
                };

                List<BehaviourTrigger> triggers = new List<BehaviourTrigger>();
                List<EfeuMessage> messages = new List<EfeuMessage>();

                // run behaviour
                BehaviourRuntime runtime = BehaviourRuntime.RunTrigger(trigger, message);

                if (runtime.Result == BehaviourRuntimeResult.Skipped)
                {
                    continue;
                }
                else
                {
                    foreach (BehaviourTrigger outTrigger in runtime.Triggers)
                    {
                        await behaviourTriggerCommands.CreateAsync(outTrigger, triggerEntity.DefinitionVersionId);
                    }

                    foreach (EfeuMessage outMessage in runtime.Messages)
                    {
                        await CreateEffect(outMessage);
                    }
                }
            }
        }

        public async Task PublishAsync(int definitionId, BehaviourDefinitionStep[] steps)
        {
            // clear all static triggers for old definition
            BehaviourDefinitionVersionEntity? definitionVersion = await behaviourDefinitionRepository.GetNewestVersionAsync(definitionId);
            if (definitionVersion == null)
                throw new Exception();

            await behaviourTriggerCommands.DeleteStaticAsync(definitionVersion.Id);

            // create new version
            int newDefinitionVersionId = await behaviourDefinitionCommands.CreateVersionAsync(new BehaviourDefinitionVersionEntity()
            {
                DefinitionId = definitionVersion.DefinitionId,
                Version = definitionVersion.Version + 1,
                Steps = steps
            });

            BehaviourRuntime runtime = BehaviourRuntime.Run(steps, Guid.NewGuid());

            foreach (BehaviourTrigger outTrigger in runtime.Triggers)
            {
                await behaviourTriggerCommands.CreateAsync(outTrigger, newDefinitionVersionId);
            }

            foreach (EfeuMessage outMessage in runtime.Messages)
            {
                await CreateEffect(outMessage);
            }
        }

        public Task DeleteAsync(int id)
        {
            return behaviourEffectRepository.DeleteAsync(id);
        }
    }
}
