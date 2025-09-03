using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Trigger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public class WorkflowRuntimeExport
    {
        public WorkflowRuntimeState State;
        public SomeData Input = new SomeData();
        public SomeData Output = new SomeData();
        public IDictionary<int, SomeData> MethodData = new Dictionary<int, SomeData>();
        public IDictionary<int, SomeData> MethodOutput = new Dictionary<int, SomeData>();

        public int CurrentMethodId;
        public SomeData DispatchResult;
        public Stack<int> ReturnStack = new Stack<int>();
    }

    public class WorkflowRuntimeScope
    {
        public WorkflowRuntimeScope? Parent;
        public int CurrentMethodId;
        public bool MethodHasRun;
        public SomeData CurrentMethodData;
        public SomeData LastMethodOutput;
        public SomeData DispatchResult;
        public WorkflowMethodState State;
        public WorkflowTriggerHash Trigger;
        public Dictionary<int, SomeData> MethodOutput = [];
        public List<WorkflowRuntimeScope> Children = [];
    }

    public class WorkflowRuntime
    {
        public WorkflowRuntimeState State => state;

        private WorkflowRuntimeState state = WorkflowRuntimeState.Initial;

        private HashSet<WorkflowRuntimeScope> activeScopes = [];

        private readonly WorkflowDefinition definition;

        private readonly IWorkflowMethodProvider methodProvider;
        private readonly IWorkflowFunctionProvider functionProvider;
        private readonly IWorkflowTriggerProvider triggerProvider;

        public WorkflowRuntime(WorkflowDefinition definition, IWorkflowMethodProvider methodProvider, IWorkflowFunctionProvider functionProvider, IWorkflowTriggerProvider triggerProvider, SomeData input = default) 
        {
            this.definition = definition;
            this.methodProvider = methodProvider;
            this.functionProvider = functionProvider;
            this.triggerProvider = triggerProvider;
        }

        public WorkflowRuntime(WorkflowRuntimeExport import, WorkflowDefinition definition, IWorkflowMethodProvider methodProvider, IWorkflowFunctionProvider functionProvider, IWorkflowTriggerProvider triggerProvider, SomeData input = default)
        {
            this.state = import.State;
            this.definition = definition;
            this.methodProvider = methodProvider;
            this.functionProvider = functionProvider;
            this.triggerProvider = triggerProvider;
        }

        public Task<WorkflowTriggerDescriptor[]> AttachAsync()
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            throw new NotImplementedException();
        }

        private Task InitializeAsync(CancellationToken token = default)
        {
            int startMethodId = definition.Actions.Where(x => 
                x.Type == WorkflowActionNodeType.Method && definition.EntryPoints.Contains(x.Id))
                .FirstOrDefault()?.Id ?? 0;

            if (startMethodId == 0)
            {
                state = WorkflowRuntimeState.Done;
                return Task.CompletedTask;
            }
            else
            {
                activeScopes.Add(new WorkflowRuntimeScope()
                {
                    State = WorkflowMethodState.Running,
                    CurrentMethodId = startMethodId,
                });
                state = WorkflowRuntimeState.Running;
                return Task.CompletedTask;
            }
        }

        public Task StepAsync(CancellationToken token = default)
        {
            if (state == WorkflowRuntimeState.Initial)
                return InitializeAsync(token);

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
                return StepAsync(scope, token);
            }
        }

        /// <summary>
        /// Starts the workflow from a trigger
        /// </summary>
        /// <param name="id"></param>
        /// <param name="signal"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Trigger(int id, object signal)
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            // send signal to trigger with id
            // prepare workflow to be started with this trigger
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
                    SomeData input = GetInputForMethod(scope, actionNode);
                    WorkflowMethodContext context =  new WorkflowMethodContext(input, scope.CurrentMethodData, scope.DispatchResult);
                    IWorkflowMethod methodInstance = methodProvider.GetMethod(actionNode.Name);
                    WorkflowMethodState newState = methodInstance.Signal(context, signal);
                    MoveNext(scope, newState, context, actionNode);
                    if (scope.State == WorkflowMethodState.Running)
                    {
                        state = WorkflowRuntimeState.Running;
                    }
                }
            }
        }

        private async Task StepAsync(WorkflowRuntimeScope scope, CancellationToken token)
        {
            if (scope.State != WorkflowMethodState.Running)
                throw new InvalidOperationException();

            WorkflowActionNode actionNode = definition.GetAction(scope.CurrentMethodId);
            SomeData input = GetInputForMethod(scope, actionNode);
            WorkflowMethodContext context;
            if (scope.MethodHasRun)
            {
                context = new WorkflowMethodContext(input, scope.CurrentMethodData, scope.DispatchResult);
            }
            else
            {
                context = new WorkflowMethodContext(input);
            }

            IWorkflowMethod methodInstance = methodProvider.GetMethod(actionNode.Name);
            WorkflowMethodState newState = WorkflowMethodState.Running;
            try
            {
                newState = await methodInstance.RunAsync(context, token);
            }
            catch (Exception)
            {
                if (actionNode.ErrorRoute == 0)
                {
                    throw;
                }
                else
                {
                    scope.CurrentMethodId = actionNode.ErrorRoute;
                    scope.State = WorkflowMethodState.Running;
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
                scope.MethodHasRun = true;
                scope.CurrentMethodData = context.Data;
            }
            else if (scope.State == WorkflowMethodState.Suspended)
            {
                scope.MethodHasRun = true;
                scope.CurrentMethodData = context.Data;
                scope.Trigger = context.Trigger;
            }
            else if (scope.State == WorkflowMethodState.Yield)
            {
                scope.CurrentMethodData = context.Data;
                scope.MethodHasRun = true;
                BeginScope(scope, context.Output, actionNode.DispatchRoute);
            }
            else if (scope.State == WorkflowMethodState.Done)
            {
                scope.LastMethodOutput = context.Output;
                int nextMethodId = 0;
                if (context.Route == null)
                {
                    nextMethodId = actionNode.DefaultRoute;
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
                    scope.MethodHasRun = false;
                }
            }
        }

        private void BeginScope(WorkflowRuntimeScope scope, SomeData input, int initialMethodId)
        {
            // create a new child scope
            WorkflowRuntimeScope newScope = new WorkflowRuntimeScope()
            {
                Parent = scope,
                CurrentMethodId = initialMethodId,
                MethodHasRun = false,
                State = WorkflowMethodState.Running,
            };

            scope.Children.Add(newScope);
            activeScopes.Remove(scope);
            activeScopes.Add(newScope);
        }

        private void EndScope(WorkflowRuntimeScope scope, SomeData result)
        {
            // report back to parent scope
            if (scope.Parent == null)
            {
                
            }
            else
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

        private SomeData GetFunctionOutput(WorkflowRuntimeScope scope, WorkflowActionNode node)
        {
            InputEvaluationContext inputContext = new InputEvaluationContext((id) => GetOutput(scope, id), default);
            SomeData input = node.Input.EvaluateInput(inputContext);

            string methodname = node.Name;
            IWorkflowFunction workflowFunctionInstance = functionProvider.GetFunction(methodname);
            WorkflowFunctionContext context = new WorkflowFunctionContext();
            SomeData outputs = workflowFunctionInstance.Run(context, input);
            return outputs;
        }

        private SomeData GetInputForMethod(WorkflowRuntimeScope scope, WorkflowActionNode method)
        {
            InputEvaluationContext context = new InputEvaluationContext((id) => GetOutput(scope, id), scope.LastMethodOutput);
            return method.Input.EvaluateInput(context);
        }

        private SomeData GetOutput(WorkflowRuntimeScope scope, int id)
        {
            WorkflowActionNode node = definition.GetAction(id);
            return node.Type switch
            {
                WorkflowActionNodeType.Function => GetFunctionOutput(scope, node),
                WorkflowActionNodeType.Method => GetMethodOutput(scope, node),
                WorkflowActionNodeType.Start => GetMethodOutput(scope, node),
                WorkflowActionNodeType.Trigger => GetMethodOutput(scope, node),
                WorkflowActionNodeType.Write => GetMethodOutput(scope, node),
                WorkflowActionNodeType.Fork => GetMethodOutput(scope, node),
                // WorkflowActionNodeType.Join => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        private SomeData GetMethodOutput(WorkflowRuntimeScope start, WorkflowActionNode node)
        {
            WorkflowRuntimeScope? scope = start;
            while (scope != null)
            {
                if (scope.MethodOutput.TryGetValue(node.Id, out SomeData value))
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
                State = this.State
            };
        }
    }
}
