using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
using Efeu.Runtime.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Runtime
{
    public enum WorkflowInstanceState
    {
        Initial,
        Running,
        Suspended,
        Done
    }

    public class WorkflowInstanceExport
    {
        public WorkflowInstanceState State;
        public int CurrentMethodId;
        public SomeData Input = new SomeData();
        public SomeData Output = new SomeData();
        public SomeStruct Variables = new SomeStruct();
        public IDictionary<int, SomeData> MethodData = new Dictionary<int, SomeData>();
        public IDictionary<int, SomeData> MethodOutput = new Dictionary<int, SomeData>();
        public SomeData DispatchResult;
        public Stack<int> ReturnStack = new Stack<int>();
    }

    public class WorkflowInstance
    {
        public WorkflowInstanceState State => state;

        private int currentMethodId;

        private WorkflowInstanceState state = WorkflowInstanceState.Initial;
        private SomeData workflowInput;
        private SomeData workflowOutput;
        private SomeStruct variables;
        private IDictionary<int, SomeData> methodData;
        private IDictionary<int, SomeData> methodOutput;
        private SomeData dispatchResult;
        private Stack<int> returnStack;

        private WorkflowDefinition definition;

        private IWorkflowMethod currentMethodInstance;
        private IWorkflowActionInstanceFactory instanceFactory;

        public WorkflowInstance(WorkflowDefinition definition, IWorkflowActionInstanceFactory instanceFactory, SomeData input = default) 
        {
            this.definition = definition;
            this.instanceFactory = instanceFactory;
            this.methodData = new Dictionary<int, SomeData>();
            this.methodOutput = new Dictionary<int, SomeData>();
            this.currentMethodId = definition.EntryPointId;
            this.workflowInput = input;
            this.workflowOutput = new SomeData();
            this.variables = new SomeStruct();
            this.returnStack = new Stack<int>();
            this.dispatchResult = new SomeData();
        }

        public WorkflowInstance(WorkflowInstanceExport data, WorkflowDefinition definition, IWorkflowActionInstanceFactory instanceFactory)
        {
            this.state = data.State;
            this.currentMethodId = data.CurrentMethodId;
            this.variables = data.Variables;
            this.methodData = data.MethodData;
            this.methodOutput = data.MethodOutput;
            this.definition = definition;
            this.instanceFactory = instanceFactory;
            this.workflowInput = data.Input;
            this.workflowOutput = data.Output;
            this.dispatchResult = data.DispatchResult;
            this.returnStack = data.ReturnStack;
        }

        public Task StepAsync(CancellationToken token = default)
        {
            return state switch {
                WorkflowInstanceState.Initial => InitializeAsync(token),
                WorkflowInstanceState.Running => RunMethodAsync(token),
                _ => throw new InvalidOperationException()
            };
        }

        public void SendSignal(CustomWorkflowSignal signal)
        {
            if (state != WorkflowInstanceState.Suspended)
                throw new InvalidOperationException();

            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeData input = GetInputForMethod(actionNode);
            WorkflowMethodContext context = new WorkflowMethodContext(variables, input, methodData.GetValueOrDefault(currentMethodId), dispatchResult);
            WorkflowMethodState methodState;
            try
            {
                methodState = currentMethodInstance.OnSignal(context, signal);
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
                state = WorkflowInstanceState.Running;
                return;
            }
            else if (methodState == WorkflowMethodState.Suspended)
            {
                state = WorkflowInstanceState.Suspended;
                return;
            }
        }

        private Task InitializeAsync(CancellationToken token = default)
        {
            if (currentMethodId == 0)
            {
                state = WorkflowInstanceState.Done;
                return Task.CompletedTask;
            }
            else
            {
                string methodname = definition.GetAction(currentMethodId).Name;
                currentMethodInstance = instanceFactory.GetMethodInstance(methodname);
                state = WorkflowInstanceState.Running;
                return Task.CompletedTask;
            }
        }

        private void DispatchMethod()
        {
            returnStack.Push(currentMethodId);
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);

            currentMethodId = actionNode.DispatchRoute;
            string methodname = definition.GetAction(currentMethodId).Name;
            currentMethodInstance = instanceFactory.GetMethodInstance(methodname);
        }

        private async Task RunMethodAsync(CancellationToken token)
        {
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeData input = GetInputForMethod(actionNode);
            WorkflowMethodContext context;
            if (methodData.ContainsKey(currentMethodId))
            {
                context = new WorkflowMethodContext(variables, input, methodData.GetValueOrDefault(currentMethodId), dispatchResult);
            }
            else
            {
                context = new WorkflowMethodContext(variables, input);
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
                state = WorkflowInstanceState.Suspended;
                return;
            }
            else if (methodState == WorkflowMethodState.Running)
            {
                state = WorkflowInstanceState.Running;
                return;
            }
            else if (methodState == WorkflowMethodState.Dispatch)
            {
                state = WorkflowInstanceState.Running;
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
                state = WorkflowInstanceState.Done;
                return;
            }
            else
            {
                // move to next
                currentMethodId = nextRef;

                string methodname = definition.GetAction(currentMethodId).Name;
                currentMethodInstance = instanceFactory.GetMethodInstance(methodname);

                state = WorkflowInstanceState.Running;
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
                currentMethodInstance = instanceFactory.GetMethodInstance(methodname);

                state = WorkflowInstanceState.Running;
                return;
            }
        }

        private SomeData GetFunctionOutput(WorkflowActionNode node)
        {
            SomeData input = GetInputForFunction(node);

            string methodname = node.Name;
            IWorkflowFunctionInstance workflowFunctionInstance = instanceFactory.GetFunctionInstance(methodname);
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
                variables, workflowInput, GetOutput);

            return method.Input.EvaluateInput(context);
        }

        private SomeData GetOutput(int id)
        {
            WorkflowActionNode node = definition.GetAction(id);
            return node.Type switch
            {
                WorkflowActionNodeType.Function => GetFunctionOutput(node),
                WorkflowActionNodeType.Method => GetMethodOutput(node),
                // WorkflowActionNodeType.Task => throw new NotImplementedException(),
                // WorkflowActionNodeType.WaitTask => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        private SomeData GetMethodOutput(WorkflowActionNode node)
        {
            return methodOutput[node.Id];
        }

        public WorkflowInstanceExport Export()
        {
            return new WorkflowInstanceExport()
            {
                State = this.State,
                CurrentMethodId = this.currentMethodId,
                Input = workflowInput,
                Variables = variables,
                MethodData = methodData,
                MethodOutput = methodOutput,
                Output = workflowOutput,
                DispatchResult = dispatchResult,
                ReturnStack = returnStack
            };
        }
    }
}
