using Antlr4.Build.Tasks;
using Efeu.Runtime;
using Efeu.Runtime.Script;
using Efeu.Runtime.Value;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public enum EfeuRuntimeResult
    {
        Executed,
        Skipped
    }

    public class EfeuRuntime
    {
        public readonly EfeuBehaviourStep[] Steps = [];
        public readonly Guid Id;
        public readonly List<EfeuTrigger> Triggers = [];
        public readonly List<EfeuMessage> Messages = [];

        public readonly DateTimeOffset Now = DateTimeOffset.Now;

        public readonly bool IsImmediate;

        public EfeuRuntimeResult Result => result;


        private EfeuRuntimeResult result;

        private readonly EfeuMessage triggerSignal = new EfeuMessage();

        private readonly EfeuTrigger trigger = new EfeuTrigger();

        public EfeuRuntime(EfeuBehaviourStep[] steps, Guid id, int behaviourId)
        {
            this.Steps = steps;
            this.Id = id;
            this.IsImmediate = true;
            this.trigger.BehaviourId = behaviourId;
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

        public static EfeuRuntime Run(EfeuBehaviourStep[] steps, Guid id, int behaviourId = 0)
        {
            EfeuRuntime runtime = new EfeuRuntime(steps, id, behaviourId);
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
                EfeuBehaviourStep step = trigger.Step;
                if (!TriggerMatchesMessage(trigger, triggerSignal, step))
                {
                    return EfeuRuntimeResult.Skipped;
                }

                EfeuRuntimeScope scope = trigger.Scope
                    .With("now", Now)
                    .With("input", triggerSignal.Payload);

                EfeuBehaviourStep[] steps = step.Do;
                RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
            }

            return EfeuRuntimeResult.Executed;
        }

        private static bool TriggerMatchesMessage(EfeuTrigger trigger, EfeuMessage signal, EfeuBehaviourStep step)
        {
            return signal.Tag == trigger.Tag &&
                   signal.Type == trigger.Type &&
                   signal.Matter == trigger.Matter;
        }

        private void RunSteps(EfeuBehaviourStep[] steps, string position, EfeuRuntimeScope parentScope)
        {
            var (lets, remaining) = steps.Partition((item) => item.Kind == EfeuBehaviourStepKind.Let);

            EfeuRuntimeScope scope = parentScope;
            foreach (EfeuBehaviourStep step in lets)
            {
                scope = scope.With(step.Name, step.Input.Evaluate(scope));
            }

            int i = 0;
            foreach (EfeuBehaviourStep step in remaining)
            {
                RunStep(step, $"{position}/{i}", scope);
                i++;
            }
        }

        private void RunStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
        {
            if (step.Kind == EfeuBehaviourStepKind.Emit)
            {
                RunEmitStep(step, position, scope);
            }
            else if (step.Kind == EfeuBehaviourStepKind.Raise)
            {
                RunRaiseStep(step, position, scope);
            }
            else if (step.Kind == EfeuBehaviourStepKind.If)
            {
                RunIfStep(step, position, scope);
            }
            else if (step.Kind == EfeuBehaviourStepKind.Unless)
            {
                RunUnlessStep(step, position, scope);
            }
            else if (step.Kind == EfeuBehaviourStepKind.For)
            {
                RunForStep(step, position, scope);
            }
            else if (step.Kind == EfeuBehaviourStepKind.Await)
            {
                RunAwaitStep(step, position, scope);
            }
            else if (step.Kind == EfeuBehaviourStepKind.On)
            {
                RunOnStep(step, position, scope);
            }
        }

        private void RunEmitStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
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
                    Position = position,
                    BehaviourId = trigger.BehaviourId,
                    Matter = messageId,
                    Step = step,
                });
            }

            Messages.Add(new EfeuMessage()
            {
                Id = messageId,
                CorrelationId = Id,
                Type = step.Name,
                Tag = EfeuMessageTag.Effect,
                Matter = messageId,
                Payload = step.Input.Evaluate(scope)
            });
        }

        private void RunRaiseStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
        {
            Messages.Add(new EfeuMessage()
            {
                Id = Guid.NewGuid(),
                CorrelationId = Id,
                Type = step.Name,
                Tag = EfeuMessageTag.Data,
                Payload = step.Input.Evaluate(scope)
            });
        }

        private void RunIfStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
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

        private void RunUnlessStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
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

        private void RunForStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
        {
            foreach (EfeuValue item in step.Input.Evaluate(scope).Each())
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunAwaitStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
        {
            Triggers.Add(new EfeuTrigger()
            {
                Id = Guid.NewGuid(),
                CorrelationId = Id,
                Scope = scope,
                Tag = EfeuMessageTag.Data,
                Type = step.Name,
                Position = position,
                Input = step.Input.Evaluate(scope),
                BehaviourId = trigger.BehaviourId,
                Step = step
            });
        }

        private void RunOnStep(EfeuBehaviourStep step, string position, EfeuRuntimeScope scope)
        {
            if (!IsImmediate)
                throw new InvalidOperationException("Static Triggers (On) is only available in immediate mode!");

            Triggers.Add(new EfeuTrigger()
            {
                Id = Guid.NewGuid(),
                Scope = scope,
                Tag = EfeuMessageTag.Data,
                Type = step.Name,
                BehaviourId = trigger.BehaviourId,
                Input = step.Input.Evaluate(scope),
                Position = position,
                Step = step
            });
        }
    }
}
