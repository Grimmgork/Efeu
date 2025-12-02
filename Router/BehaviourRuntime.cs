using Efeu.Runtime;
using Efeu.Runtime.Data;
using Efeu.Runtime.Model;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public class EfeuMessage
    {
        public string Name = "";

        public EfeuMessageTag Tag;

        public Guid CorrelationId; // from wich it came

        public Guid TriggerId; // for wich it is commonly used for a response

        public EfeuValue Data;
    }

    public enum EfeuMessageTag
    {
        Effect,
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
        public Guid Id;

        public Guid CorrelationId;

        public string Position = ""; // position of trigger row /0/Else/1

        public BehaviourScope Scope = new BehaviourScope(); // Scope around trigger row

        public string MessageName = "";

        public EfeuMessageTag MessageTag;

        public BehaviourDefinitionStep Step = new BehaviourDefinitionStep();

        public int DefinitionId;

        public bool IsStatic => CorrelationId == Guid.Empty; // a trigger is static if it is not assigned to a instance
    }

    public class BehaviourScope
    {
        public readonly BehaviourScope? Parent;

        public readonly ImmutableDictionary<string, EfeuValue> Constants;

        public BehaviourScope(ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Constants = constants;
        }

        public BehaviourScope()
        {
            this.Constants = ImmutableDictionary<string, EfeuValue>.Empty;
        }

        public BehaviourScope(BehaviourScope parent, ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Parent = parent;
            this.Constants = constants;
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
    }

    public enum BehaviourRuntimeResult
    {
        Executed,
        Skipped
    }

    public class BehaviourRuntime
    {
        public readonly BehaviourDefinitionStep[] Steps = [];
        public readonly Guid Id;
        public readonly List<BehaviourTrigger> Triggers = [];
        public readonly List<EfeuMessage> Messages = [];

        public BehaviourRuntimeResult Result => result;

        public readonly bool IsImmediate;

        
        private BehaviourRuntimeResult result;

        private readonly EfeuMessage triggerMessage = new EfeuMessage();

        private readonly BehaviourTrigger trigger = new BehaviourTrigger();

        public BehaviourRuntime(BehaviourDefinitionStep[] steps, Guid id, int definitionId)
        {
            this.Steps = steps;
            this.Id = id;
            this.IsImmediate = true;
            this.trigger.DefinitionId = definitionId;
        }

        public BehaviourRuntime(BehaviourTrigger trigger, EfeuMessage message, Guid id)
        {
            this.Id = id;
            this.triggerMessage = message;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public BehaviourRuntime(BehaviourTrigger trigger, EfeuMessage message)
        {
            this.Id = trigger.CorrelationId;
            this.triggerMessage = message;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public static BehaviourRuntime Run(BehaviourDefinitionStep[] steps, Guid id, int definitionId = 0)
        {
            BehaviourRuntime runtime = new BehaviourRuntime(steps, id, definitionId);
            runtime.result = runtime.Execute();
            return runtime;
        }

        public static BehaviourRuntime RunStaticTrigger(BehaviourTrigger trigger, EfeuMessage message, Guid id)
        {
            if (!trigger.IsStatic)
                throw new InvalidOperationException("Trigger must be static!");

            BehaviourRuntime runtime = new BehaviourRuntime(trigger, message, id);
            runtime.result = runtime.Execute();
            return runtime;
        }

        public static BehaviourRuntime RunTrigger(BehaviourTrigger trigger, EfeuMessage message)
        {
            if (trigger.IsStatic)
                throw new InvalidOperationException("Trigger must not be static!");

            BehaviourRuntime runtime = new BehaviourRuntime(trigger, message);
            runtime.result = runtime.Execute();
            return runtime;
        }

        private BehaviourRuntimeResult Execute()
        {
            if (IsImmediate)
            {
                BehaviourScope scope = new BehaviourScope(); // root scope
                RunSteps(Steps, "", scope);
            }
            else
            {
                // trigger continuation
                BehaviourDefinitionStep step = trigger.Step;
                if (!TriggerMatchesMessage(trigger, triggerMessage, step))
                {
                    return BehaviourRuntimeResult.Skipped;
                }

                ImmutableDictionary<string, EfeuValue> constants = ImmutableDictionary<string, EfeuValue>.Empty.Add("input", triggerMessage.Data);
                BehaviourScope scope = new BehaviourScope(trigger.Scope, constants);

                BehaviourDefinitionStep[] steps = step.Do;
                RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
            }

            return BehaviourRuntimeResult.Executed;
        }

        private static bool TriggerMatchesMessage(BehaviourTrigger trigger, EfeuMessage message, BehaviourDefinitionStep step)
        {
            return message.Tag == trigger.MessageTag &&
                    message.Name == trigger.MessageName;
        }

        private void RunSteps(BehaviourDefinitionStep[] steps, string position, BehaviourScope parentScope)
        {
            var (lets, remaining) = steps.Partition((item) => item.Type == BehaviourStepType.Let);

            var constants = ImmutableDictionary<string, EfeuValue>.Empty;
            foreach (BehaviourDefinitionStep step in lets)
            {
                BehaviourScope scope = new BehaviourScope(parentScope, constants);
                BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
                constants = constants.Add(step.Name, step.Input(context));
            }

            BehaviourScope finalScope = new BehaviourScope(parentScope, constants);
            int i = 0;
            foreach (BehaviourDefinitionStep step in remaining)
            {
                RunStep(step, $"{position}/{i}", finalScope);
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
        }

        private void RunEmitStep(BehaviourDefinitionStep step, BehaviourScope scope)
        {
            Messages.Add(new EfeuMessage()
            {
                CorrelationId = Id,
                Name = step.Name,
                Tag = EfeuMessageTag.Effect
            });
        }

        private void RunIfStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            if (step.Input(context))
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
            else
            {
                RunSteps(step.Else, $"{position}/Else", scope);
            }
        }

        private void RunUnlessStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            if (step.Input(context))
            {
                RunSteps(step.Else, $"{position}/Else", scope);
            }
            else
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunForStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            BehaviourExpressionContext context = new BehaviourExpressionContext(scope);
            foreach (EfeuValue item in step.Input(context).Each())
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunCallStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            Guid triggerId = Guid.NewGuid();

            Messages.Add(new EfeuMessage()
            {
                CorrelationId = Id,
                Name = step.Name,
                Tag = EfeuMessageTag.Effect,
                TriggerId = triggerId,
            });

            Triggers.Add(new BehaviourTrigger()
            {
                Id = triggerId,
                CorrelationId = Id,
                Scope = scope,
                MessageTag = EfeuMessageTag.Response,
                MessageName = step.Name,
                Position = position,
                DefinitionId = trigger.DefinitionId,
                Step = step,
            });
        }

        private void RunAwaitStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            Triggers.Add(new BehaviourTrigger()
            {
                Id = Guid.NewGuid(),
                CorrelationId = Id,
                Scope = scope,
                MessageTag = EfeuMessageTag.Effect,
                MessageName = step.Name,
                Position = position,
                DefinitionId = trigger.DefinitionId,
                Step = step
            });
        }

        private void RunOnStep(BehaviourDefinitionStep step, string position, BehaviourScope scope)
        {
            if (!IsImmediate)
                throw new InvalidOperationException("Static Triggers (On) is only available in immediate mode!");

            Triggers.Add(new BehaviourTrigger()
            {
                Id = Guid.NewGuid(),
                Scope = scope,
                MessageTag = EfeuMessageTag.Effect,
                MessageName = step.Name,
                DefinitionId = trigger.DefinitionId,
                Position = position
            });
        }

        
    }
}
