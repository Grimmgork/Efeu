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
        private readonly IBehaviourTriggerCommands behaviourTriggerCommands;
        private readonly IBehaviourTriggerRepository behaviourTriggerRepository;

        public BehaviourEffectCommands(IUnitOfWork unitOfWork, IBehaviourEffectRepository behaviourEffectRepository, IBehaviourDefinitionRepository behaviourDefinitionRepository, IBehaviourTriggerCommands behaviourTriggerCommands, IBehaviourTriggerRepository behaviourTriggerRepository)
        {
            this.unitOfWork = unitOfWork;
            this.behaviourEffectRepository = behaviourEffectRepository;
            this.behaviourTriggerCommands = behaviourTriggerCommands;
            this.behaviourTriggerRepository = behaviourTriggerRepository;
            this.behaviourDefinitionRepository = behaviourDefinitionRepository;
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
            // load matching triggers
            BehaviourTriggerEntity[] triggerEntities = await behaviourTriggerRepository.GetMatchingAsync(message.Name, message.Tag);
            BehaviourDefinitionEntity[] definitionEntities = await behaviourDefinitionRepository.GetByIdsAsync(triggerEntities.Select(i => i.DefinitionId).ToArray());

            BehaviourTrigger[] triggers = triggerEntities.Select(i => new BehaviourTrigger()
            {
                Id = i.Id,
                CorrelationId = i.CorrelationId,
                MessageName = i.MessageName,
                MessageTag = i.MessageTag,
                Scope = i.Scope,
                DefinitionId = i.DefinitionId,
                Position = i.Position,
            }).ToArray();

            Dictionary<int, BehaviourDefinition> definitions = definitionEntities.ToDictionary(i => i.Id, i => new BehaviourDefinition()
            {
                Id = i.Id,
                Name = i.Name,
                Version = i.Version,
                Steps = i.Steps,
            });

            List<BehaviourTrigger> newTriggers = new List<BehaviourTrigger>();
            List<EfeuMessage> newMessages = new List<EfeuMessage>();

            foreach (BehaviourTrigger trigger in triggers)
            {
                // run behaviour
                BehaviourDefinition definition = definitions[trigger.DefinitionId];
                BehaviourRuntime runtime = BehaviourRuntime.RunTrigger(definition, trigger, message);

                if (runtime.Result == BehaviourRuntimeResult.Skipped)
                {
                    continue;
                }
                else
                {
                    newTriggers.AddRange(runtime.Triggers);
                    newMessages.AddRange(runtime.Messages);
                }
            }

            foreach (BehaviourTrigger newTrigger in newTriggers)
            {
                await behaviourTriggerCommands.CreateAsync(newTrigger);
            }

            foreach (EfeuMessage newMessage in newMessages)
            {
                await CreateEffect(message);
            }
        }

        public Task RunImmediate(BehaviourDefinition behaviourDefinition, int oldDefinitionId)
        {
            // clear all static triggers for old definition
            // run in immediate mode
            // apply all triggers
            // create effects
        }

        public Task DeleteAsync(int id)
        {
            return behaviourEffectRepository.DeleteAsync(id);
        }
    }
}
