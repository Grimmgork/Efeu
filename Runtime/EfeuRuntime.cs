using Efeu.Runtime;
using Efeu.Runtime.Script;
using Efeu.Runtime.Value;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public class EfeuRuntime
    {
        public List<EfeuMessage> Messages = [];
        public List<EfeuTrigger> Triggers = [];

        public DateTimeOffset Now;

        public bool IsImmediate;
        public int BehaviourId;
        public Guid CorrelationId;
        public Guid CausationId;

        public bool IsSkipped;

        public EfeuRuntime(Guid correlationId, Guid causationId, int behaviourId, bool isImmediate, DateTimeOffset now)
        {
            this.CorrelationId = correlationId;
            this.IsImmediate = isImmediate;
            this.BehaviourId = behaviourId;
            this.CausationId = causationId;
            this.Now = now;
        }

        public static EfeuRuntime Run(EfeuBehaviourStep[] steps, int behaviourId, DateTimeOffset timestamp)
        {
            EfeuRuntime runtime = new EfeuRuntime(Guid.NewGuid(), Guid.NewGuid(), behaviourId, true, timestamp);
            runtime.Execute(steps);
            return runtime;
        }

        public static EfeuRuntime RunTrigger(EfeuTrigger trigger, EfeuMessage message)
        {
            EfeuRuntime runtime = new EfeuRuntime(trigger.IsStatic ? Guid.NewGuid() : trigger.CorrelationId, message.Id, trigger.BehaviourId, false, message.Timestamp);
            runtime.ExecuteTrigger(trigger, message);
            return runtime;
        }

        private void Execute(EfeuBehaviourStep[] steps)
        {
            if (!IsImmediate)
                throw new InvalidOperationException();

            EfeuRuntimeScope scope = EfeuRuntimeScope.Empty
                    .With("now", Now);

            RunSteps(steps, "", scope);
        }

        private void ExecuteTrigger(EfeuTrigger trigger, EfeuMessage message)
        {
            if (IsImmediate)
                throw new InvalidOperationException();

            // trigger continuation
            if (!TriggerMatchesMessage(trigger, message))
            {
                IsSkipped = true;
                return;
            }

            EfeuRuntimeScope scope = trigger.Scope
                .With("now", Now)
                .With("@", message.Payload);

            EfeuBehaviourStep[] steps = trigger.Step.Do;
            RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
        }

        private static bool TriggerMatchesMessage(EfeuTrigger trigger, EfeuMessage message)
        {
            return message.Timestamp >= trigger.CreationTime &&
                   message.Tag == trigger.Tag &&
                   message.Type == trigger.Type &&
                   message.Matter == trigger.Matter;
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
            Guid messageId = Guid.NewGuid();
            Guid group = Guid.NewGuid();
            if (step.Do.Length > 0)
            {
                Triggers.Add(new EfeuTrigger()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = CorrelationId,
                    CreationTime = Now,
                    Scope = scope,
                    Tag = EfeuMessageTag.Result,
                    Position = position,
                    BehaviourId = BehaviourId,
                    Matter = messageId,
                    Group = group,
                    Step = step,
                });
            }

            if (step.Error.Length > 0)
            {
                Triggers.Add(new EfeuTrigger()
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = CorrelationId,
                    CreationTime = Now,
                    Scope = scope,
                    Tag = EfeuMessageTag.Fault,
                    Position = position,
                    BehaviourId = BehaviourId,
                    Matter = messageId,
                    Group = group,
                    Step = step,
                });
            }

            Messages.Add(new EfeuMessage()
            {
                Id = messageId,
                CorrelationId = CorrelationId,
                Timestamp = Now,
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
                CorrelationId = CorrelationId,
                Timestamp = Now,
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
                CorrelationId = CorrelationId,
                CreationTime = Now,
                Scope = scope,
                Tag = EfeuMessageTag.Data,
                Type = step.Name,
                Position = position,
                Input = step.Input.Evaluate(scope),
                BehaviourId = BehaviourId,
                Step = step,
                Group = Guid.NewGuid()
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
                CreationTime = Now,
                Tag = EfeuMessageTag.Data,
                Type = step.Name,
                BehaviourId = BehaviourId,
                CorrelationId = Guid.Empty,
                Input = step.Input.Evaluate(scope),
                Position = position,
                Step = step,
                Group = Guid.NewGuid()
            });
        }
    }
}
