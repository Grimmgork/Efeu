using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Trigger;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public enum WorkflowRuntimeState
    {
        Initial,
        Running,
        Suspended,
        Done
    }

    [MessagePackObject]
    public class WorkflowRuntimeExport
    {
        [Key(0)]
        public WorkflowRuntimeState State;
        [Key(1)]
        public WorkflowRuntimeScopeExport RootScope = new WorkflowRuntimeScopeExport();
        [IgnoreMember]
        public EfeuValue Output => RootScope.LastMethodOutput;
    }

    [MessagePackObject]
    public class WorkflowRuntimeScopeExport
    {
        [Key(0)]
        public int CurrentMethodId;
        [Key(1)]
        public EfeuValue CurrentMethodData;
        [Key(2)]
        public int Iterations;
        [Key(3)]
        public EfeuValue LastMethodOutput;
        [Key(4)]
        public EfeuValue DispatchResult;
        [Key(5)]
        public WorkflowMethodState State;
        [Key(6)]
        public WorkflowTriggerHash Trigger;
        [Key(7)]
        public Dictionary<int, EfeuValue> MethodOutput = [];
        [Key(8)]
        public Dictionary<string, EfeuValue> Variables = [];
        [Key(9)]
        public List<WorkflowRuntimeScopeExport> Children = [];
    }

    public class WorkflowRuntimeScope
    {
        public WorkflowRuntimeScope? Parent;
        public int CurrentMethodId;
        public int Iterations;
        public EfeuValue CurrentMethodData; 
        public EfeuValue LastMethodOutput;
        public EfeuValue DispatchResult; 
        public WorkflowMethodState State;
        public WorkflowTriggerHash Trigger;
        public Dictionary<int, EfeuValue> MethodOutput = [];
        public Dictionary<string, EfeuValue> Variables = [];
        public List<WorkflowRuntimeScope> Children = [];

        public WorkflowRuntimeScopeExport Export()
        {
            return new WorkflowRuntimeScopeExport()
            {
                CurrentMethodId = CurrentMethodId,
                CurrentMethodData = CurrentMethodData,
                Iterations = Iterations,
                LastMethodOutput = LastMethodOutput,
                DispatchResult = DispatchResult,
                State = State,
                Trigger = Trigger,
                MethodOutput = MethodOutput,
                Variables = Variables,
                Children = Children.Select(x => x.Export()).ToList()
            };
        }

        public static WorkflowRuntimeScope Import(WorkflowRuntimeScopeExport export, WorkflowRuntimeScope? parent = null)
        {
            WorkflowRuntimeScope scope = new WorkflowRuntimeScope()
            {
                Parent = parent,
                CurrentMethodId = export.CurrentMethodId,
                CurrentMethodData = export.CurrentMethodData,
                Iterations = export.Iterations,
                LastMethodOutput = export.LastMethodOutput,
                DispatchResult = export.DispatchResult,
                State = export.State,
                Trigger = export.Trigger,
                MethodOutput = export.MethodOutput,
                Variables = export.Variables,
            };

            scope.Children = export.Children.Select((i) => WorkflowRuntimeScope.Import(i, scope)).ToList();
            return scope;
        }
    }

    public class WorkflowRuntime
    {
        public WorkflowRuntimeState State => state;

        public EfeuValue Output => rootScope.LastMethodOutput;

        private WorkflowRuntimeState state = WorkflowRuntimeState.Initial;

        private WorkflowRuntimeScope rootScope = new WorkflowRuntimeScope();
        private HashSet<WorkflowRuntimeScope> activeScopes = [];

        private WorkflowDefinition definition;
        private WorkflowRuntimeEnvironment environment;

        private WorkflowRuntime(WorkflowRuntimeEnvironment environment, WorkflowDefinition definition)
        {
            this.environment = environment;
            this.definition = definition;
        }

        public static WorkflowRuntime Prepare(WorkflowRuntimeEnvironment environment, WorkflowDefinition definition, EfeuValue input = default)
        {
            WorkflowRuntime runtime = new WorkflowRuntime(environment, definition);
            runtime.Prepare(input);
            return runtime;
        }

        private void Prepare(EfeuValue input)
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            int startMethodId = definition.Start;
            if (startMethodId == 0)
            {
                state = WorkflowRuntimeState.Done;
            }
            else
            {
                WorkflowActionNode startActionNode = definition.GetAction(startMethodId);
                if (startActionNode.Type == WorkflowActionNodeType.Start)
                {
                    rootScope = new WorkflowRuntimeScope()
                    {
                        MethodOutput = new Dictionary<int, EfeuValue>([
                            new (startActionNode.Id, input)
                        ]),
                        State = WorkflowMethodState.Running,
                        CurrentMethodId = startActionNode.Then
                    };
                }
                else
                {
                    rootScope = new WorkflowRuntimeScope()
                    {
                        State = WorkflowMethodState.Running,
                        CurrentMethodId = startActionNode.Id
                    };
                }

                activeScopes.Add(rootScope);
                state = WorkflowRuntimeState.Running;
            }
        }

        public static WorkflowRuntime PrepareTrigger(WorkflowRuntimeEnvironment environment, WorkflowDefinition definition, int striggerId, object signal)
        {
            WorkflowRuntime runtime = new WorkflowRuntime(environment, definition);
            runtime.PrepareTrigger(striggerId, signal);
            return runtime;
        }

        private void PrepareTrigger(int startId, object signal)
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            // prepare workflow to run started by a trigger
        }

        public static WorkflowTriggerDescriptor[] GetTriggers(WorkflowRuntimeEnvironment environment, WorkflowDefinition definition)
        {
            WorkflowRuntime runtime = new WorkflowRuntime(environment, definition);
            return runtime.GetTriggers();
        }

        private WorkflowTriggerDescriptor[] GetTriggers()
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            // find all triggers and evaluate its inputs
            return [];
        }

        public Task StepAsync(CancellationToken token = default)
        {
            if (state != WorkflowRuntimeState.Running)
                throw new InvalidOperationException();

            WorkflowRuntimeScope? scope = activeScopes.FirstOrDefault(s => s.State == WorkflowMethodState.Running);
            if (scope == null)
            {
                if (activeScopes.Any(s => s.State == WorkflowMethodState.Suspended))
                {
                    state = WorkflowRuntimeState.Suspended;
                    return Task.CompletedTask;
                }
                else
                {
                    state = WorkflowRuntimeState.Done;
                    return Task.CompletedTask;
                }
            }
            else
            {
                return StepScopeAsync(scope, token);
            }
        }

        public void Signal(WorkflowTriggerHash hash, object signal)
        {
            WorkflowTriggerHash[] hashes = hash.Expand();

            // send signals to all suspended threads
            foreach (WorkflowRuntimeScope scope in activeScopes)
            {
                if (scope.State == WorkflowMethodState.Suspended && hashes.Contains(scope.Trigger))
                {
                    WorkflowActionNode actionNode = definition.GetAction(scope.CurrentMethodId);
                    EfeuValue input = GetInputForMethod(scope, actionNode);
                    WorkflowMethodContext context =  new WorkflowMethodContext(input, scope.CurrentMethodData, scope.DispatchResult, scope.Iterations);
                    IWorkflowMethod methodInstance = environment.MethodProvider.GetMethod(actionNode.Name);
                    WorkflowMethodState newState = methodInstance.Signal(context, signal);
                    MoveNext(scope, newState, context, actionNode);
                    if (scope.State == WorkflowMethodState.Running)
                    {
                        state = WorkflowRuntimeState.Running;
                    }
                }
            }
        }

        private async Task StepScopeAsync(WorkflowRuntimeScope scope, CancellationToken token)
        {
            if (scope.State != WorkflowMethodState.Running)
                throw new InvalidOperationException();

            WorkflowActionNode actionNode = definition.GetAction(scope.CurrentMethodId);
            EfeuValue input = GetInputForMethod(scope, actionNode);
            WorkflowMethodContext context;
            if (scope.Iterations == 0)
            {
                // run method for first time
                context = new WorkflowMethodContext(input);
            }
            else
            {
                // run method multiple times
                context = new WorkflowMethodContext(input, scope.CurrentMethodData, scope.DispatchResult, scope.Iterations);
            }

            IWorkflowMethod methodInstance = environment.MethodProvider.GetMethod(actionNode.Name);
            WorkflowMethodState newState = WorkflowMethodState.Running;
            try
            {
                newState = await methodInstance.RunAsync(context, token);
                scope.Iterations++;
            }
            catch (Exception)
            {
                if (actionNode.Error == 0)
                {
                    throw;
                }
                else
                {
                    scope.CurrentMethodId = actionNode.Error;
                    return;
                }
            }

            MoveNext(scope, newState, context, actionNode);
        }

        private void MoveNext(WorkflowRuntimeScope scope, WorkflowMethodState newState, WorkflowMethodContext context, WorkflowActionNode actionNode)
        {
            scope.State = newState;
            if (scope.State == WorkflowMethodState.Running)
            {
                scope.CurrentMethodData = context.Data;
            }
            else if (scope.State == WorkflowMethodState.Suspended)
            {
                scope.CurrentMethodData = context.Data;
                scope.Trigger = context.Trigger;
            }
            else if (scope.State == WorkflowMethodState.Yield)
            {
                scope.CurrentMethodData = context.Data;
                BeginScope(scope, context.Output, actionNode.Dispatch);
            }
            else if (scope.State == WorkflowMethodState.Done)
            {
                scope.LastMethodOutput = context.Output;
                scope.MethodOutput[scope.CurrentMethodId] = context.Output;
                scope.Iterations = 0;

                int nextMethodId = 0;
                if (context.Route == null)
                {
                    nextMethodId = actionNode.Then;
                }
                else
                {
                    nextMethodId = actionNode.Routes.GetValueOrDefault(context.Route);
                }

                if (nextMethodId == 0)
                {
                    EndScope(scope, context.Output);
                }
                else
                {
                    // continue if there is a next one
                    scope.State = WorkflowMethodState.Running;
                    scope.CurrentMethodId = nextMethodId;
                    scope.CurrentMethodData = EfeuValue.Nil();
                }
            }
        }

        private void BeginScope(WorkflowRuntimeScope scope, EfeuValue input, int initialMethodId)
        {
            // create a new child scope
            WorkflowRuntimeScope newScope = new WorkflowRuntimeScope()
            {
                Parent = scope,
                CurrentMethodId = initialMethodId,
                State = WorkflowMethodState.Running,
            };

            scope.Children.Add(newScope);
            activeScopes.Remove(scope);
            activeScopes.Add(newScope);
        }

        private void EndScope(WorkflowRuntimeScope scope, EfeuValue result)
        {
            // report back to parent scope
            if (scope.Parent != null)
            {
                if (scope.Parent.State != WorkflowMethodState.Yield)
                    throw new InvalidOperationException();

                scope.Parent.DispatchResult = result;
                scope.Parent.State = WorkflowMethodState.Running;
                scope.Parent.Children.Remove(scope);
                activeScopes.Remove(scope);
                activeScopes.Add(scope.Parent);
            }
        }

        private EfeuValue GetFunctionOutput(WorkflowRuntimeScope scope, WorkflowActionNode node)
        {
            InputEvaluationContext inputContext = new InputEvaluationContext((id) => GetOutput(scope, id), (name) => GetVariable(scope, name), default);
            EfeuValue input = node.Input.EvaluateInput(inputContext);

            string methodname = node.Name;
            IWorkflowFunction workflowFunctionInstance = environment.FunctionProvider.GetFunction(methodname);
            WorkflowFunctionContext context = new WorkflowFunctionContext();
            EfeuValue outputs = workflowFunctionInstance.Run(context, input);
            return outputs;
        }

        private EfeuValue GetInputForMethod(WorkflowRuntimeScope scope, WorkflowActionNode method)
        {
            InputEvaluationContext context = new InputEvaluationContext((id) => GetOutput(scope, id), (name) => GetVariable(scope, name), scope.LastMethodOutput);
            return method.Input.EvaluateInput(context);
        }

        private EfeuValue GetVariable(WorkflowRuntimeScope start, string name)
        {
            WorkflowRuntimeScope? scope = start;
            while (scope != null)
            {
                if (scope.Variables.TryGetValue(name, out EfeuValue value))
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

        private void SetVariable(WorkflowRuntimeScope start, string name, EfeuValue value)
        {
            WorkflowRuntimeScope? scope = start;
            while (scope != null)
            {
                if (scope.Variables.ContainsKey(name))
                {
                    scope.Variables[name] = value;
                    return;
                }
                else
                {
                    scope = scope.Parent;
                }
            }

            start.Variables[name] = value;
        }

        private EfeuValue GetOutput(WorkflowRuntimeScope scope, int id)
        {
            WorkflowActionNode node = definition.GetAction(id);
            return node.Type switch
            {
                // WorkflowActionNodeType.Function => GetFunctionOutput(scope, node),
                WorkflowActionNodeType.Call => GetMethodOutput(scope, node),
                // WorkflowActionNodeType.Start => GetMethodOutput(scope, node),
                WorkflowActionNodeType.Trigger => GetMethodOutput(scope, node),
                WorkflowActionNodeType.Start => GetMethodOutput(scope, node),
                // WorkflowActionNodeType.Join => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        private EfeuValue GetMethodOutput(WorkflowRuntimeScope start, WorkflowActionNode node)
        {
            WorkflowRuntimeScope? scope = start;
            while (scope != null)
            {
                if (scope.MethodOutput.TryGetValue(node.Id, out EfeuValue value))
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

        public WorkflowRuntimeExport Export()
        {
            return new WorkflowRuntimeExport()
            {
                State = this.State,
                RootScope = this.rootScope.Export()
            };
        }

        public static WorkflowRuntime Import(WorkflowRuntimeEnvironment environment, WorkflowDefinition definition, WorkflowRuntimeExport export)
        {
            WorkflowRuntime runtime = new WorkflowRuntime(environment, definition);
            runtime.Import(export);
            return runtime;
        }

        private void Import(WorkflowRuntimeExport export)
        {
            this.state = export.State;
            this.rootScope = WorkflowRuntimeScope.Import(export.RootScope);
        }
    }
}
