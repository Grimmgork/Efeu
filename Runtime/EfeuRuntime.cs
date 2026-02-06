using Efeu.Runtime;
using Efeu.Runtime.Value;
using Efeu.Runtime.Script;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace Efeu.Runtime
{
    public class EfeuRuntimeScope
    {
        public readonly ImmutableDictionary<string, EfeuValue> Constants;

        public static EfeuRuntimeScope Empty { get; } = new EfeuRuntimeScope();

        public EfeuRuntimeScope()
        {
            Constants = ImmutableDictionary<string, EfeuValue>.Empty;
        }

        public EfeuRuntimeScope(ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Constants = constants;
        }

        public EfeuValue Get(string name)
        {
            return Constants[name];
        }

        public EfeuRuntimeScope With(string name, EfeuValue value)
        {
            return new EfeuRuntimeScope(Constants.SetItem(name, value));
        }
    }

    public enum EfeuRuntimeResult
    {
        Executed,
        Skipped
    }

    public class EfeuRuntime
    {
        public readonly BehaviourDefinitionStep[] Steps = [];
        public readonly Guid Id;
        public readonly List<EfeuTrigger> Triggers = [];
        public readonly List<EfeuMessage> Messages = [];

        public readonly DateTimeOffset Now = DateTimeOffset.Now;

        public EfeuRuntimeResult Result => result;

        public readonly bool IsImmediate;

        
        private EfeuRuntimeResult result;

        private readonly EfeuMessage triggerSignal = new EfeuMessage();

        private readonly EfeuTrigger trigger = new EfeuTrigger();

        public EfeuRuntime(BehaviourDefinitionStep[] steps, Guid id, int definitionId)
        {
            this.Steps = steps;
            this.Id = id;
            this.IsImmediate = true;
            this.trigger.DefinitionId = definitionId;
        }

        public EfeuRuntime(EfeuTrigger trigger, EfeuMessage signal, Guid id)
        {
            this.Id = id;
            this.triggerSignal = signal;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public EfeuRuntime(EfeuTrigger trigger, EfeuMessage signal)
        {
            this.Id = trigger.CorrelationId;
            this.triggerSignal = signal;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public static EfeuRuntime Run(BehaviourDefinitionStep[] steps, Guid id, int definitionId = 0)
        {
            EfeuRuntime runtime = new EfeuRuntime(steps, id, definitionId);
            runtime.result = runtime.Execute();
            return runtime;
        }

        public static EfeuRuntime RunStaticTrigger(EfeuTrigger trigger, EfeuMessage signal, Guid id)
        {
            if (!trigger.IsStatic)
                throw new InvalidOperationException("Trigger must be static!");

            EfeuRuntime runtime = new EfeuRuntime(trigger, signal, id);
            runtime.result = runtime.Execute();
            return runtime;
        }

        public static EfeuRuntime RunTrigger(EfeuTrigger trigger, EfeuMessage signal)
        {
            if (trigger.IsStatic)
                throw new InvalidOperationException("Trigger must not be static!");

            EfeuRuntime runtime = new EfeuRuntime(trigger, signal);
            runtime.result = runtime.Execute();
            return runtime;
        }

        private EfeuRuntimeResult Execute()
        {
            if (IsImmediate)
            {
                EfeuRuntimeScope scope = EfeuRuntimeScope.Empty
                    .With("now", Now);

                RunSteps(Steps, "", scope);
            }
            else
            {
                // trigger continuation
                BehaviourDefinitionStep step = trigger.Step;
                if (!TriggerMatchesMessage(trigger, triggerSignal, step))
                {
                    return EfeuRuntimeResult.Skipped;
                }

                EfeuRuntimeScope scope = trigger.Scope
                    .With("now", Now)
                    .With("input", triggerSignal.Data);

                BehaviourDefinitionStep[] steps = step.Do;
                RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
            }

            return EfeuRuntimeResult.Executed;
        }

        private static bool TriggerMatchesMessage(EfeuTrigger trigger, EfeuMessage signal, BehaviourDefinitionStep step)
        {
            return signal.Tag == trigger.Tag &&
                   signal.Name == trigger.Name &&
                   signal.Matter == trigger.Matter;
        }

        private void RunSteps(BehaviourDefinitionStep[] steps, string position, EfeuRuntimeScope parentScope)
        {
            var (lets, remaining) = steps.Partition((item) => item.Type == BehaviourStepType.Let);

            EfeuRuntimeScope scope = parentScope;
            foreach (BehaviourDefinitionStep step in lets)
            {
                scope = scope.With(step.Name, step.Input.Evaluate(scope));
            }

            int i = 0;
            foreach (BehaviourDefinitionStep step in remaining)
            {
                RunStep(step, $"{position}/{i}", scope);
                i++;
            }
        }

        private void RunStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            if (step.Type == BehaviourStepType.Emit)
            {
                RunEmitStep(step, position, scope);
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
            else if (step.Type == BehaviourStepType.On)
            {
                RunOnStep(step, position, scope);
            }
        }

        private void RunEmitStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            Guid triggerId = Guid.Empty;
            Guid messageId = Guid.NewGuid();
            if (step.Do.Length > 0)
            {
                triggerId = Guid.NewGuid();
                Triggers.Add(new EfeuTrigger()
                {
                    Id = triggerId,
                    CorrelationId = Id,
                    Scope = scope,
                    Tag = EfeuMessageTag.Result,
                    Name = step.Name,
                    Position = position,
                    DefinitionId = trigger.DefinitionId,
                    Matter = messageId,
                    Step = step,
                });
            }

            Messages.Add(new EfeuMessage()
            {
                Id = messageId,
                CorrelationId = Id,
                Name = step.Name,
                Tag = EfeuMessageTag.Effect
            });
        }

        private void RunIfStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            if (step.Input.Evaluate(scope))
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
            else
            {
                RunSteps(step.Else, $"{position}/Else", scope);
            }
        }

        private void RunUnlessStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            if (step.Input.Evaluate(scope))
            {
                RunSteps(step.Else, $"{position}/Else", scope);
            }
            else
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunForStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            foreach (EfeuValue item in step.Input.Evaluate(scope).Each())
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunAwaitStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            Triggers.Add(new EfeuTrigger()
            {
                Id = Guid.NewGuid(),
                CorrelationId = Id,
                Scope = scope,
                Tag = EfeuMessageTag.Data,
                Name = step.Name,
                Position = position,
                Input = step.Input.Evaluate(scope),
                DefinitionId = trigger.DefinitionId,
                Step = step
            });
        }

        private void RunOnStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            if (!IsImmediate)
                throw new InvalidOperationException("Static Triggers (On) is only available in immediate mode!");

            Triggers.Add(new EfeuTrigger()
            {
                Id = Guid.NewGuid(),
                Scope = scope,
                Tag = EfeuMessageTag.Data,
                Name = step.Name,
                DefinitionId = trigger.DefinitionId,
                Input = step.Input.Evaluate(scope),
                Position = position
            });
        }
    }
}
