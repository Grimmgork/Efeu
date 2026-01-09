using Efeu.Router;
using Efeu.Router.Value;
using Efeu.Router.Script;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public class EfeuExpressionContext : IEfeuScope
    {
        private readonly IEfeuScope scope;

        public EfeuExpressionContext(IEfeuScope scope)
        {
            this.scope = scope;
        }

        public EfeuExpressionContext()
        {
            this.scope = EfeuScriptScope.Empty;
        }

        public EfeuValue Get(string name) => scope.Get(name);
    }


    public class EfeuRuntimeScope : IEfeuScope
    {
        public readonly EfeuRuntimeScope? Parent;

        public readonly ImmutableDictionary<string, EfeuValue> Constants;

        public EfeuRuntimeScope(ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Constants = constants;
        }

        public EfeuRuntimeScope()
        {
            this.Constants = ImmutableDictionary<string, EfeuValue>.Empty;
        }

        public EfeuRuntimeScope(EfeuRuntimeScope parent, ImmutableDictionary<string, EfeuValue> constants)
        {
            this.Parent = parent;
            this.Constants = constants;
        }

        public EfeuValue Get(string name)
        {
            EfeuRuntimeScope? scope = this;
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

        public EfeuRuntimeResult Result => result;

        public readonly bool IsImmediate;

        
        private EfeuRuntimeResult result;

        private readonly EfeuSignal triggerSignal = new EfeuSignal();

        private readonly EfeuTrigger trigger = new EfeuTrigger();

        public EfeuRuntime(BehaviourDefinitionStep[] steps, Guid id, int definitionId)
        {
            this.Steps = steps;
            this.Id = id;
            this.IsImmediate = true;
            this.trigger.DefinitionId = definitionId;
        }

        public EfeuRuntime(EfeuTrigger trigger, EfeuSignal signal, Guid id)
        {
            this.Id = id;
            this.triggerSignal = signal;
            this.trigger = trigger;
            this.IsImmediate = false;
        }

        public EfeuRuntime(EfeuTrigger trigger, EfeuSignal signal)
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

        public static EfeuRuntime RunStaticTrigger(EfeuTrigger trigger, EfeuSignal signal, Guid id)
        {
            if (!trigger.IsStatic)
                throw new InvalidOperationException("Trigger must be static!");

            EfeuRuntime runtime = new EfeuRuntime(trigger, signal, id);
            runtime.result = runtime.Execute();
            return runtime;
        }

        public static EfeuRuntime RunTrigger(EfeuTrigger trigger, EfeuSignal signal)
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
                EfeuRuntimeScope scope = new EfeuRuntimeScope();
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

                ImmutableDictionary<string, EfeuValue> constants = ImmutableDictionary<string, EfeuValue>.Empty.Add("input", triggerSignal.Data);
                EfeuRuntimeScope scope = new EfeuRuntimeScope(trigger.Scope, constants);

                BehaviourDefinitionStep[] steps = step.Do;
                RunSteps(steps, $"{trigger.Position}/Do", scope); // Assumption: all trigger continuations are done in the Do route
            }

            return EfeuRuntimeResult.Executed;
        }

        private static bool TriggerMatchesMessage(EfeuTrigger trigger, EfeuSignal signal, BehaviourDefinitionStep step)
        {
            return signal.Tag == trigger.Tag &&
                   signal.Name == trigger.Name &&
                   signal.TriggerId == Guid.Empty || signal.TriggerId == trigger.Id;
        }

        private void RunSteps(BehaviourDefinitionStep[] steps, string position, EfeuRuntimeScope parentScope)
        {
            var (lets, remaining) = steps.Partition((item) => item.Type == BehaviourStepType.Let);

            var constants = ImmutableDictionary<string, EfeuValue>.Empty;
            foreach (BehaviourDefinitionStep step in lets)
            {
                EfeuRuntimeScope scope = new EfeuRuntimeScope(parentScope, constants);
                EfeuExpressionContext context = new EfeuExpressionContext(scope);
                constants = constants.Add(step.Name, step.Input.Evaluate(context));
            }

            EfeuRuntimeScope finalScope = new EfeuRuntimeScope(parentScope, constants);
            int i = 0;
            foreach (BehaviourDefinitionStep step in remaining)
            {
                RunStep(step, $"{position}/{i}", finalScope);
                i++;
            }
        }

        private void RunStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
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

        private void RunEmitStep(BehaviourDefinitionStep step, EfeuRuntimeScope scope)
        {
            Messages.Add(new EfeuMessage()
            {
                CorrelationId = Id,
                Name = step.Name,
                Tag = EfeuMessageTag.Outbox
            });
        }

        private void RunIfStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            EfeuExpressionContext context = new EfeuExpressionContext(scope);
            if (step.Input.Evaluate(context))
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
            EfeuExpressionContext context = new EfeuExpressionContext(scope);
            if (step.Input.Evaluate(context))
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
            EfeuExpressionContext context = new EfeuExpressionContext(scope);
            foreach (EfeuValue item in step.Input.Evaluate(context).Each())
            {
                RunSteps(step.Do, $"{position}/Do", scope);
            }
        }

        private void RunCallStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            Guid triggerId = Guid.NewGuid();

            Messages.Add(new EfeuMessage()
            {
                CorrelationId = Id,
                Name = step.Name,
                Tag = EfeuMessageTag.Outbox,
                TriggerId = triggerId,
            });

            Triggers.Add(new EfeuTrigger()
            {
                Id = triggerId,
                CorrelationId = Id,
                Scope = scope,
                Tag = EfeuMessageTag.Completion,
                Name = step.Name,
                Position = position,
                DefinitionId = trigger.DefinitionId,
                Step = step,
            });
        }

        private void RunAwaitStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            EfeuExpressionContext context = new EfeuExpressionContext(scope);
            Triggers.Add(new EfeuTrigger()
            {
                Id = Guid.NewGuid(),
                CorrelationId = Id,
                Scope = scope,
                Tag = EfeuMessageTag.Signal,
                Name = step.Name,
                Position = position,
                Input = step.Input.Evaluate(context),
                DefinitionId = trigger.DefinitionId,
                Step = step
            });
        }

        private void RunOnStep(BehaviourDefinitionStep step, string position, EfeuRuntimeScope scope)
        {
            if (!IsImmediate)
                throw new InvalidOperationException("Static Triggers (On) is only available in immediate mode!");

            EfeuExpressionContext context = new EfeuExpressionContext(scope);
            Triggers.Add(new EfeuTrigger()
            {
                Id = Guid.NewGuid(),
                Scope = scope,
                Tag = EfeuMessageTag.Signal,
                Name = step.Name,
                DefinitionId = trigger.DefinitionId,
                Input = step.Input.Evaluate(context),
                Position = position
            });
        }
    }
}
