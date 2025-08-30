using Efeu.Integration.Logic;
using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using System;
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
        public int CurrentMethodId;
        public SomeData Input = new SomeData();
        public SomeData Output = new SomeData();
        public IDictionary<int, SomeData> MethodData = new Dictionary<int, SomeData>();
        public IDictionary<int, SomeData> MethodOutput = new Dictionary<int, SomeData>();
        public SomeData DispatchResult;
        public Stack<int> ReturnStack = new Stack<int>();
    }

    public class WorkflowRuntimeThread
    {
        public Stack<int> ReturnStack = new Stack<int>();

        public int CurrentMethodId;

        public SomeData DispatchResult;

        public List<WorkflowRuntimeThread> Threads = new List<WorkflowRuntimeThread>();
    }

    public class WorkflowRuntime
    {
        public WorkflowRuntimeState State => state;

        private int currentMethodId;

        private WorkflowRuntimeState state = WorkflowRuntimeState.Initial;
        private SomeData workflowInput;
        private SomeData workflowOutput;
        private IDictionary<int, SomeData> methodData;
        private IDictionary<int, SomeData> methodOutput;

        private SomeData dispatchResult;
        private Stack<int> returnStack;

        private WorkflowDefinition definition;

        private IWorkflowMethod currentMethodInstance;

        private IWorkflowMethodProvider methodProvider;
        private IWorkflowFunctionProvider functionProvider;
        private IWorkflowTriggerProvider triggerProvider;

        public WorkflowRuntime(WorkflowDefinition definition, IWorkflowMethodProvider methodProvider, IWorkflowFunctionProvider functionProvider, IWorkflowTriggerProvider triggerProvider, SomeData input = default) 
        {
            this.definition = definition;
            this.methodProvider = methodProvider;
            this.functionProvider = functionProvider;
            this.methodData = new Dictionary<int, SomeData>();
            this.methodOutput = new Dictionary<int, SomeData>();
            this.currentMethodId = definition.StartId;
            this.workflowInput = input;
            this.workflowOutput = new SomeData();
            this.returnStack = new Stack<int>();
            this.dispatchResult = new SomeData();
        }

        public WorkflowRuntime(WorkflowRuntimeExport import, WorkflowDefinition definition, IWorkflowMethodProvider methodProvider, IWorkflowFunctionProvider functionProvider, IWorkflowTriggerProvider triggerProvider, SomeData input = default)
        {
            this.state = import.State;
            this.currentMethodId = import.CurrentMethodId;
            this.methodData = import.MethodData;
            this.methodOutput = import.MethodOutput;
            this.definition = definition;
            this.methodProvider = methodProvider;
            this.functionProvider = functionProvider;
            this.workflowInput = import.Input;
            this.workflowOutput = import.Output;
            this.dispatchResult = import.DispatchResult;
            this.returnStack = import.ReturnStack;
        }

        public Task<WorkflowTriggerDescriptor[]> AttachAsync()
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            // get all triggers
            // attach them all
            // return descriptors

            throw new NotImplementedException();
        }

        public Task StepAsync(CancellationToken token = default)
        {
            if (state == WorkflowRuntimeState.Initial)
                return InitializeAsync(token);

            if (state != WorkflowRuntimeState.Running)
                throw new InvalidOperationException();

            return RunMethodAsync(token);
        }

        private Task InitializeAsync(CancellationToken token = default)
        {
            if (currentMethodId == 0)
            {
                state = WorkflowRuntimeState.Done;
                return Task.CompletedTask;
            }
            else
            {
                string methodname = definition.GetAction(currentMethodId).Name;
                currentMethodInstance = methodProvider.GetMethod(methodname);
                state = WorkflowRuntimeState.Running;
                return Task.CompletedTask;
            }
        }

        public void Trigger(int id, object signal)
        {
            if (state != WorkflowRuntimeState.Initial)
                throw new InvalidOperationException();

            // send signal to trigger with id
        }

        public void Signal(object signal)
        {
            if (state != WorkflowRuntimeState.Suspended)
                throw new InvalidOperationException();

            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeData input = GetInputForMethod(actionNode);
            WorkflowMethodContext context = new WorkflowMethodContext(input, methodData.GetValueOrDefault(currentMethodId), dispatchResult);
            WorkflowMethodState methodState;
            try
            {
                methodState = currentMethodInstance.Signal(context, signal);
            }
            catch (Exception exception)
            {
                MoveToHandleError(exception, actionNode.ErrorRoute);
                return;
            }

            methodData[currentMethodId] = context.Data;

            if (methodState == WorkflowMethodState.Done)
            {
                methodOutput[currentMethodId] = context.Output;

                int nextMethodId = string.IsNullOrWhiteSpace(context.Route) ? 
                    actionNode.DefaultRoute : actionNode.Routes[context.Route];
                MoveNextMethodOrDone(nextMethodId, context.Output);
                return;
            }
            else if (methodState == WorkflowMethodState.Running)
            {
                state = WorkflowRuntimeState.Running;
                return;
            }
            else if (methodState == WorkflowMethodState.Suspended)
            {
                state = WorkflowRuntimeState.Suspended;
                return;
            }
        }


        private void DispatchMethod()
        {
            returnStack.Push(currentMethodId);
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);

            currentMethodId = actionNode.DispatchRoute;
            string methodname = definition.GetAction(currentMethodId).Name;
            currentMethodInstance = methodProvider.GetMethod(methodname);
        }

        private async Task RunMethodAsync(CancellationToken token)
        {
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeData input = GetInputForMethod(actionNode);
            WorkflowMethodContext context;
            if (methodData.ContainsKey(currentMethodId))
            {
                context = new WorkflowMethodContext(input, methodData.GetValueOrDefault(currentMethodId), dispatchResult);
            }
            else
            {
                context = new WorkflowMethodContext(input);
            }

            WorkflowMethodState methodState;
            try
            {
                methodState = await currentMethodInstance.RunAsync(context, token);
            }
            catch (Exception exception)
            {
                MoveToHandleError(exception, actionNode.ErrorRoute);
                return;
            }

            methodData[currentMethodId] = context.Data;

            if (methodState == WorkflowMethodState.Done)
            {
                methodOutput[currentMethodId] = context.Output;
                methodData.Remove(currentMethodId);

                int nextMethodId = string.IsNullOrWhiteSpace(context.Route) ?
                    actionNode.DefaultRoute : actionNode.Routes[context.Route];
                MoveNextMethodOrDone(nextMethodId, context.Output);
                return;
            }
            else if (methodState == WorkflowMethodState.Suspended)
            {
                state = WorkflowRuntimeState.Suspended;
                return;
            }
            else if (methodState == WorkflowMethodState.Running)
            {
                state = WorkflowRuntimeState.Running;
                return;
            }
            else if (methodState == WorkflowMethodState.Dispatch)
            {
                state = WorkflowRuntimeState.Running;
                methodOutput[currentMethodId] = context.Output;
                DispatchMethod();
                return;
            }
        }

        private void MoveNextMethodOrDone(int nextRef, SomeData lastOutput)
        {
            if (nextRef == 0 && returnStack.Any())
            {
                dispatchResult = lastOutput;
                nextRef = returnStack.Pop();
            }

            // resolve next
            if (nextRef == 0)
            {
                workflowOutput = lastOutput;
                state = WorkflowRuntimeState.Done;
                return;
            }
            else
            {
                // move to next
                currentMethodId = nextRef;

                string methodname = definition.GetAction(currentMethodId).Name;
                currentMethodInstance = methodProvider.GetMethod(methodname);

                state = WorkflowRuntimeState.Running;
                return;
            }
        }

        private void MoveToHandleError(Exception exception, int errorHandleRef)
        {
            // resolve next
            if (errorHandleRef == 0)
            {
                throw exception;
            }
            else
            {
                // move to next
                currentMethodId = errorHandleRef;

                string methodname = definition.GetAction(currentMethodId).Name;
                currentMethodInstance = methodProvider.GetMethod(methodname);

                state = WorkflowRuntimeState.Running;
                return;
            }
        }

        private SomeData GetFunctionOutput(WorkflowActionNode node)
        {
            SomeData input = GetInputForFunction(node);

            string methodname = node.Name;
            IWorkflowFunction workflowFunctionInstance = functionProvider.GetFunction(methodname);
            WorkflowFunctionContext context = new WorkflowFunctionContext();
            SomeData outputs = workflowFunctionInstance.Run(context, input);
            return outputs;
        }

        private SomeData GetInputForFunction(WorkflowActionNode function)
        {
            return GetInputForMethod(function);
        }

        private SomeData GetInputForMethod(WorkflowActionNode method)
        {
            InputEvaluationContext context = new InputEvaluationContext(
                workflowInput, GetOutput);

            return method.Input.EvaluateInput(context);
        }

        private SomeData GetOutput(int id)
        {
            WorkflowActionNode node = definition.GetAction(id);
            return node.Type switch
            {
                WorkflowActionNodeType.Function => GetFunctionOutput(node),
                WorkflowActionNodeType.Method => GetMethodOutput(node),
                WorkflowActionNodeType.Start => GetMethodOutput(node),
                WorkflowActionNodeType.Trigger => GetMethodOutput(node),
                // WorkflowActionNodeType.Task => throw new NotImplementedException(),
                // WorkflowActionNodeType.WaitTask => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        private SomeData GetMethodOutput(WorkflowActionNode node)
        {
            return methodOutput[node.Id];
        }

        public WorkflowRuntimeExport Export()
        {
            return new WorkflowRuntimeExport()
            {
                State = this.State,
                CurrentMethodId = this.currentMethodId,
                Input = workflowInput,
                MethodData = methodData,
                MethodOutput = methodOutput,
                Output = workflowOutput,
                DispatchResult = dispatchResult,
                ReturnStack = returnStack
            };
        }
    }
}
