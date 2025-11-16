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

        public string InstanceId = ""; // from wich it came

        public EfeuValue Data;
    }

    public enum EfeuMessageTag
    {
        Data,
        Response,
        Error
    }

    public class BehaviourExpressionContext
    {
        private BehaviourScope scope;

        public BehaviourExpressionContext(BehaviourScope scope)
        {
            this.scope = scope;
        }

        public EfeuValue Constant(string name) => scope.GetConstant(name);
    }

    public class BehaviourTrigger
    {
        public bool IsStatic => string.IsNullOrWhiteSpace(InstanceId); // a trigger is static if it is not assigned to a instance

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

    public enum BehaviourRuntimeState
    {
        Done,
        Skipped,
        Suspended
    }

    public class BehaviourRuntime
    {
        public readonly BehaviourDefinition Definition;
        public readonly string Id;
        public readonly List<BehaviourTrigger> Triggers = [];
        public readonly List<EfeuMessage> Messages = [];

        public BehaviourRuntimeState State => state;

        public readonly bool IsImmediate;

        
        private BehaviourRuntimeState state;

        private readonly EfeuMessage triggerMessage = new EfeuMessage();

        private readonly BehaviourTrigger trigger = new BehaviourTrigger();

        public BehaviourRuntime(BehaviourDefinition definition, string id)
        {
            this.Definition = definition;
            this.Id = id;
            this.IsImmediate = true;
        }

        public BehaviourRuntime(BehaviourDefinition definition, BehaviourTrigger trigger, EfeuMessage message, string id)
        {
            this.Id = id;
            this.Definition = definition;
            this.triggerMessage = message;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public BehaviourRuntime(BehaviourDefinition definition, BehaviourTrigger trigger, EfeuMessage message)
        {
            this.Id = trigger.InstanceId;
            this.Definition = definition;
            this.triggerMessage = message;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public static BehaviourRuntime Run(BehaviourDefinition definition, string id)
        {
            BehaviourRuntime runtime = new BehaviourRuntime(definition, id);
            runtime.state = runtime.Execute();
            return runtime;
        }

        public static BehaviourRuntime RunStaticTrigger(BehaviourDefinition definition, BehaviourTrigger trigger, EfeuMessage message, string id)
        {
            if (!trigger.IsStatic)
                throw new InvalidOperationException("Trigger must be static!");

            BehaviourRuntime runtime = new BehaviourRuntime(definition, trigger, message, id);
            runtime.state = runtime.Execute();
            return runtime;
        }

        public static BehaviourRuntime RunTrigger(BehaviourDefinition definition, BehaviourTrigger trigger, EfeuMessage message)
        {
            if (trigger.IsStatic)
                throw new InvalidOperationException("Trigger must not be static!");

            BehaviourRuntime runtime = new BehaviourRuntime(definition, trigger, message);
            runtime.state = runtime.Execute();
            return runtime;
        }

        private BehaviourRuntimeState Execute()
        {
            Triggers.Clear();
            Messages.Clear();

            if (IsImmediate)
            {
                BehaviourScope scope = new BehaviourScope();
                RunSteps(Definition.Steps, "", scope);
            }
            else
            {
                // trigger continuation
                BehaviourDefinitionStep step = GetPosition(Definition, trigger.Position);
                if (!TriggerMatchesMessage(trigger, triggerMessage, step))
                {
                    return BehaviourRuntimeState.Skipped;
                }

                BehaviourScope scope = new BehaviourScope(trigger.Scope);
                scope.DefineConstant("input", triggerMessage.Data);

                BehaviourDefinitionStep[] steps = step.Do;
                RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
            }

            if (Triggers.Any(t => !t.IsStatic))
            {
                return BehaviourRuntimeState.Suspended;
            }
            else
            {
                return BehaviourRuntimeState.Done;
            }
        }

        private static bool TriggerMatchesMessage(BehaviourTrigger trigger, EfeuMessage message, BehaviourDefinitionStep step)
        {
            return message.Tag == trigger.MessageTag &&
                    message.Name == trigger.MessageName;
        }

        private void RunSteps(BehaviourDefinitionStep[] steps, string position, BehaviourScope scope)
        {
            // todo run all lets first?
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
            else if (step.Type == BehaviourStepType.Unless)
            {
                RunUnlessStep(step, position, scope);
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
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            if (step.Input(context))
            {
                RunSteps(step.Do, $"{position}/Do", new BehaviourScope(scope));
            }
            else
            {
                RunSteps(step.Else, $"{position}/Else", new BehaviourScope(scope));
            }
        }

        private void RunUnlessStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            if (step.Input(context))
            {
                RunSteps(step.Else, $"{position}/Else", new BehaviourScope(scope));
            }
            else
            {
                RunSteps(step.Do, $"{position}/Do", new BehaviourScope(scope));
            }
        }

        private void RunForStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            foreach (EfeuValue item in step.Input(context).Each())
            {
                RunSteps(step.Do, $"{position}/Do", new BehaviourScope(scope));
            }
        }

        private void RunLetStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            scope.DefineConstant(step.Name, step.Input(context));
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
