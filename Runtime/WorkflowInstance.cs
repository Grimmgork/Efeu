using Efeu.Runtime.Data;
using Efeu.Runtime.Function;
using Efeu.Runtime.Message;
using Efeu.Runtime.Method;
using Efeu.Runtime.Model;
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

    public class WorkflowInstanceData
    {
        public int Id;
        public int WorkflowDefintitionId;
        public WorkflowInstanceState State;
        public int CurrentMethodId;
        public SomeData Input = new SomeData();
        public SomeData Output = new SomeData();
        public SomeStruct Variables = new SomeStruct();
        public IDictionary<int, SomeData> MethodData = new Dictionary<int, SomeData>();
        public IDictionary<int, SomeData> MethodOutput = new Dictionary<int, SomeData>();
        public Stack<int> ReturnStack = new Stack<int>();
    }

    public class WorkflowInstance
    {
        public readonly int Id;
        public readonly int WorkflowDefintitionId;
        public WorkflowInstanceState State => state;
        public int CurrentMethodId => currentMethodId;

        private int currentMethodId;

        private WorkflowInstanceState state = WorkflowInstanceState.Initial;
        private SomeData workflowInput;
        private SomeData workflowOutput;
        private SomeStruct variables;
        private IDictionary<int, SomeData> methodData;
        private IDictionary<int, SomeData> methodOutput;
        private Stack<int> returnStack;
        private WorkflowDefinition definition;

        private IWorkflowMethodInstance currentMethodInstance;
        private IWorkflowActionInstanceFactory instanceFactory;

        private SomeData doInput;

        public WorkflowInstance(int id, WorkflowDefinition definition, IWorkflowActionInstanceFactory instanceFactory, SomeData? input = null) 
        {
            this.Id = id;
            this.WorkflowDefintitionId = definition.Id;
            this.definition = definition;
            this.instanceFactory = instanceFactory;
            this.methodData = new Dictionary<int, SomeData>();
            this.methodOutput = new Dictionary<int, SomeData>();
            this.currentMethodId = definition.EntryPointId;
            this.workflowInput = input ?? new SomeData();
            this.workflowOutput = new SomeData();
            this.variables = new SomeStruct();
            this.returnStack = new Stack<int>();
        }

        public WorkflowInstance(WorkflowInstanceData data, WorkflowDefinition definition, IWorkflowActionInstanceFactory instanceFactory)
        {
            this.Id = data.Id;
            this.state = data.State;
            this.currentMethodId = data.CurrentMethodId;
            this.variables = data.Variables;
            this.methodData = data.MethodData;
            this.methodOutput = data.MethodOutput;
            this.WorkflowDefintitionId = data.WorkflowDefintitionId;
            this.definition = definition;
            this.instanceFactory = instanceFactory;
            this.workflowInput = data.Input;
            this.workflowOutput = data.Output;
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

        public void SendSignal(WorkflowSignal signal)
        {
            if (state != WorkflowInstanceState.Suspended)
                throw new InvalidOperationException();

            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeData input = GetInputForMethod(actionNode);
            WorkflowMethodContext context = new WorkflowMethodContext(variables, input, workflowOutput, methodData.GetValueOrDefault(currentMethodId));
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
                    actionNode.DefaultRoute : actionNode.Routes.First(x => x.Name == context.Route).ActionId;
                MoveNextMethodOrDone(nextMethodId);
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

        private void DoDispatch()
        {
            returnStack.Push(currentMethodId);
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);

            currentMethodId = actionNode.DoReference;
            string methodname = definition.GetAction(currentMethodId).Name;
            currentMethodInstance = instanceFactory.GetMethodInstance(methodname);
        }

        private SomeData DoFunction(SomeData input)
        {
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            if (actionNode.DoLambda != null)
                return actionNode.DoLambda(input);

            return GetFunctionOutput(actionNode.DoReference, actionNode.DoOutputTraversal);
        }

        private async Task RunMethodAsync(CancellationToken token)
        {
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeData input = GetInputForMethod(actionNode);
            WorkflowMethodContext context;
            if (methodData.ContainsKey(currentMethodId))
            {
                context = new WorkflowMethodContext(variables, input, workflowOutput, methodData.GetValueOrDefault(currentMethodId));
            }
            else
            {
                context = new WorkflowMethodContext(variables, input, workflowOutput);
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
                workflowOutput = context.WorkflowOutput;

                int nextMethodId = string.IsNullOrWhiteSpace(context.Route) ?
                    actionNode.DefaultRoute : actionNode.Routes.First(x => x.Name == context.Route).ActionId;
                MoveNextMethodOrDone(nextMethodId);
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
                DoDispatch();
                return;
            }
        }

        private void MoveNextMethodOrDone(int nextRef)
        {
            if (nextRef == 0 && returnStack.Any())
            {
                nextRef = returnStack.Pop();
            }

            // resolve next
            if (nextRef == 0)
            {
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

        private SomeData GetFunctionOutput(int id, SomeDataTraversal name = default)
        {
            WorkflowActionNode functionNode = definition.GetAction(id);
            SomeData input = GetInputForFunction(functionNode);

            string methodname = functionNode.Name;
            IWorkflowFunctionInstance workflowFunctionInstance = instanceFactory.GetFunctionInstance(methodname);
            WorkflowFunctionContext context = new WorkflowFunctionContext(DoFunction);
            SomeData outputs = workflowFunctionInstance.Run(context, input);
            return outputs.Traverse(name);
        }

        private SomeData GetInputForFunction(WorkflowActionNode function)
        {
            return GetInputForMethod(function);
        }

        private SomeData GetInputForMethod(WorkflowActionNode method)
        {
            InputEvaluationContext context = new InputEvaluationContext(
                variables, workflowInput, GetMethodOutput, GetFunctionOutput, doInput);

            return method.Input.EvaluateInput(context);
        }

        private SomeData GetMethodOutput(int id, SomeDataTraversal name)
        {
            return methodOutput[id].Traverse(name);
        }

        public WorkflowInstanceData Export()
        {
            return new WorkflowInstanceData()
            {
                Id = this.Id,
                WorkflowDefintitionId = this.WorkflowDefintitionId,
                State = this.State,
                CurrentMethodId = this.CurrentMethodId,
                Input = workflowInput,
                Variables = variables,
                MethodData = methodData,
                MethodOutput = methodOutput,
                Output = workflowOutput,
                ReturnStack = returnStack
            };
        }
    }
}
