using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public class EfeuMessage
    {
        public string Id = "";

        public string ResponseTriggerId = "";

        public EfeuMessageTag Tag;

        public string Name = "";

        public string InstanceId = "";

        public EfeuValue Data;
    }

    public enum EfeuMessageTag
    {
        Data,
        Response,
        Error
    }

    public class BehaviourTrigger
    {
        public bool IsInitial => string.IsNullOrWhiteSpace(InstanceId);

        public string Id = "";

        public string DefinitionId = "";

        public string InstanceId = "";

        public string Position = ""; // position of trigger row 0.Else.1

        public BehaviourScope Scope = new BehaviourScope(); // Scope around trigger row

        public string MessageName = "";

        public EfeuMessageTag MessageTag;
    }

    public class BehaviourScope
    {
        public BehaviourScope? Parent;

        public Dictionary<string, EfeuValue> Constants = new Dictionary<string, EfeuValue>();

        public BehaviourScope()
        {
        
        }

        public BehaviourScope(BehaviourScope parent)
        {
            this.Parent = parent;
        }

        public EfeuValue GetConstant(string name)
        {
            BehaviourScope? scope = this;
            while (scope != null)
            {
                if (scope.Constants.TryGetValue(name, out EfeuValue value))
                {
                    return value;
                }
                else
                {
                    scope = scope.Parent;
                }
            }

            return default;
        }

        public void DefineConstant(string name, EfeuValue value)
        {
            if (Constants.ContainsKey(name))
                throw new Exception();

            Constants[name] = value;
        }
    }

    public class BehaviourRuntime
    {
        public readonly BehaviourDefinition Definition;

        public readonly string Id;

        public readonly List<BehaviourTrigger> Triggers = [];

        public readonly List<EfeuMessage> Messages = [];

        // private BehaviourScope Scope = new BehaviourScope();

        // private string initialPosition = "";

        private EfeuMessage triggerMessage = new EfeuMessage();

        private BehaviourTrigger trigger = new BehaviourTrigger();

        public readonly bool IsContinuation;

        public BehaviourRuntime(BehaviourDefinition definition, string id)
        {
            this.Definition = definition;
            this.Id = id;
            this.IsContinuation = false;
        }

        public BehaviourRuntime(BehaviourDefinition definition, BehaviourTrigger trigger, EfeuMessage message)
        {
            this.Id = trigger.InstanceId;
            this.Definition = definition;
            // this.Scope.Parent = trigger.Scope;
            // this.Scope.Constants["input"] = message.Data;
            this.triggerMessage = message;
            this.trigger = trigger;
            this.IsContinuation = true;
            // this.initialPosition = trigger.Position;
        }

        public void Run()
        {
            Triggers.Clear();
            Messages.Clear();

            if (IsContinuation)
            {
                // trigger continuation
                BehaviourDefinitionStep step = GetPosition(Definition, trigger.Position);
                if (!TriggerMatchesMessage(step))
                    return;

                BehaviourScope scope = new BehaviourScope(trigger.Scope);
                scope.DefineConstant("input", triggerMessage.Data);

                BehaviourDefinitionStep[] steps = step.Do;
                RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
            }
            else
            {
                BehaviourScope scope = new BehaviourScope();
                // initial run
                RunSteps(Definition.Steps, "", scope);
            }
        }

        private bool TriggerMatchesMessage(BehaviourDefinitionStep step)
        {
            // todo: validate against step data
            return triggerMessage.InstanceId == Id &&
                triggerMessage.Tag == trigger.MessageTag &&
                triggerMessage.InstanceId == Id &&
                triggerMessage.Name ==  trigger.MessageName;
        }

        private void RunSteps(BehaviourDefinitionStep[] steps, string position, BehaviourScope scope)
        {
            // get all lets and run them
            // todo

            // run all other rows
            int i = 0;
            foreach (BehaviourDefinitionStep step in steps)
            {
                RunStep(step, $"{position}/{i}", scope);
                i++;
            }
        }

        private void RunStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            if (step.Type == BehaviourStepType.Emit)
            {
                RunEmitStep(step, scope);
            }
            else if (step.Type == BehaviourStepType.If)
            {
                RunIfStep(step, position, scope);
            }
            else if (step.Type == BehaviourStepType.For)
            {
                RunForStep(step, position, scope);
            }
            else if (step.Type == BehaviourStepType.Await)
            {
                RunAwaitStep(step, position, scope);
            }
            else if (step.Type == BehaviourStepType.Call)
            {
                RunCallStep(step, position, scope);
            }
            else if (step.Type == BehaviourStepType.On)
            {
                RunOnStep(step, position, scope);
            }
            else if (step.Type == BehaviourStepType.Let)
            {
                RunLetStep(step, position, scope);
            }
        }

        private void RunEmitStep(BehaviourDefinitionStep step, BehaviourScope scope)
        {
            Messages.Add(new EfeuMessage()
            {
                InstanceId = Id,
                Name = step.Name,
                Id = Guid.NewGuid().ToString(),
                Tag = EfeuMessageTag.Data
            });
        }

        private void RunIfStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            scope = new BehaviourScope(scope);
            if (step.Expression(scope))
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
            else
            {
                RunSteps(step.Else, $"{position}/Else", scope);
            }
        }

        private void RunForStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            foreach (EfeuValue item in step.Expression(scope).Each())
            {
                scope = new BehaviourScope(scope);
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunLetStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            scope.DefineConstant(step.Name, step.Expression(scope));
        }

        private void RunCallStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            string triggerId = Guid.NewGuid().ToString();

            Messages.Add(new EfeuMessage()
            {
                InstanceId = Id,
                Name = step.Name,
                Id = Guid.NewGuid().ToString(),
                Tag = EfeuMessageTag.Data,
                ResponseTriggerId = triggerId,
            });

            Triggers.Add(new BehaviourTrigger()
            {
                Id = triggerId,
                InstanceId = Id,
                Scope = scope,
                MessageTag = EfeuMessageTag.Data,
                MessageName = step.Name,
                Position = position,
                DefinitionId = Definition.Id,
            });
        }

        private void RunAwaitStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            Triggers.Add(new BehaviourTrigger()
            {
                Id = Guid.NewGuid().ToString(),
                InstanceId = Id,
                Scope = scope,
                MessageTag = EfeuMessageTag.Data,
                MessageName = step.Name,
                Position = position,
                DefinitionId = Definition.Id
            });
        }

        private void RunOnStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            Triggers.Add(new BehaviourTrigger()
            {
                Id = Guid.NewGuid().ToString(),
                Scope = scope,
                MessageTag = EfeuMessageTag.Data,
                MessageName = step.Name,
                Position = position,
                DefinitionId = Definition.Id
            });
        }

        private static BehaviourDefinitionStep GetPosition(BehaviourDefinition definition, string position)
        {
            // /0/Do/0/Else/2
            position = position.TrimStart('/');

            if (string.IsNullOrWhiteSpace(position))
                throw new Exception();

            string[] segments = position.Trim().Split("/");
            if (segments.Length % 2 == 0)
                throw new Exception();

            BehaviourDefinitionStep[] steps = definition.Steps;
            int index = int.Parse(segments[0]);
            BehaviourDefinitionStep step = steps[index];
            for (int i = 1; i < segments.Length; i += 2)
            {
                string path = segments[i];
                if (path == "Do")
                {
                    steps = step.Do;
                }
                if (path == "Else")
                {
                    steps = step.Else;
                }

                index = Int32.Parse(segments[i + 1]);
                step = steps[index];
            }

            return step;
        }
    }
}
