using Workflows.Data;
using Workflows.Function;
using Workflows.Message;
using Workflows.Method;
using Workflows.Model;

namespace Workflows
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
        public SomeDataStruct Input = new SomeDataStruct();
        public SomeDataStruct Output = new SomeDataStruct();
        public SomeDataStruct Variables = new SomeDataStruct();
        public IDictionary<int, SomeData> MethodData = new Dictionary<int, SomeData>();
        public IDictionary<int, SomeDataStruct> MethodOutput = new Dictionary<int, SomeDataStruct>();
    }

    // TODO handle default route
    public class WorkflowInstance
    {
        public readonly int Id;
        public readonly int WorkflowDefintitionId;
        public WorkflowInstanceState State => state;
        public int CurrentMethodId => currentMethodId;

        private int currentMethodId;

        private WorkflowInstanceState state = WorkflowInstanceState.Initial;
        private SomeDataStruct workflowInput;
        private SomeDataStruct workflowOutput;
        private SomeDataStruct variables;
        private IDictionary<int, SomeData> methodData;
        private IDictionary<int, SomeDataStruct> methodOutput;
        private WorkflowDefinition definition;

        private IWorkflowMethodInstance currentMethodInstance;
        private IWorkflowActionInstanceFactory instanceFactory;

        private Func<WorkflowSignal, Task> sendSignal;

        public WorkflowInstance(int id, WorkflowDefinition definition, IWorkflowActionInstanceFactory instanceFactory, Func<WorkflowSignal, Task> sendSignal, SomeDataStruct? input = null) 
        {
            this.Id = id;
            this.WorkflowDefintitionId = definition.Id;
            this.definition = definition;
            this.instanceFactory = instanceFactory;
            this.methodData = new Dictionary<int, SomeData>();
            this.methodOutput = new Dictionary<int, SomeDataStruct>();
            this.currentMethodId = definition.EntryPointId;
            this.workflowInput = input ?? new SomeDataStruct();
            this.workflowOutput = new SomeDataStruct();
            this.sendSignal = sendSignal;
            this.variables = new SomeDataStruct();
        }

        public WorkflowInstance(WorkflowInstanceData data, WorkflowDefinition definition, IWorkflowActionInstanceFactory instanceFactory, Func<WorkflowSignal, Task> sendSignal)
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
            this.sendSignal = sendSignal;
            this.workflowInput = data.Input;
            this.workflowOutput = data.Output;
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
            SomeDataStruct inputs = GetInputsForMethod(actionNode);
            WorkflowMethodContext context = new WorkflowMethodContext(variables, workflowOutput, inputs, sendSignal, methodData.GetValueOrDefault(currentMethodId));
            WorkflowMethodState methodState;
            try
            {
                methodState = currentMethodInstance.OnSignal(context, signal);
            }
            catch (Exception exception)
            {
                MoveToHandleError(exception, actionNode.OnError);
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

        private async Task RunMethodAsync(CancellationToken token)
        {
            WorkflowActionNode actionNode = definition.GetAction(currentMethodId);
            SomeDataStruct inputs = GetInputsForMethod(actionNode);
            WorkflowMethodContext context = new WorkflowMethodContext(variables, workflowOutput, inputs, sendSignal, methodData.GetValueOrDefault(currentMethodId));
            WorkflowMethodState methodState;

            try
            {
                methodState = await currentMethodInstance.RunAsync(context, token);
            }
            catch (Exception exception)
            {
                MoveToHandleError(exception, actionNode.OnError);
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
        }

        private void MoveNextMethodOrDone(int nextRef)
        {
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

        public InputEvaluationContext InputEvaluationContext => new InputEvaluationContext(
            variables, workflowInput, GetMethodOutput, GetFunctionOutput);

        private SomeData GetFunctionOutput(int id, SomeDataTraversal name)
        {
            WorkflowActionNode functionNode = definition.GetAction(id);
            SomeDataStruct inputs = GetInputsForFunction(functionNode);

            string methodname = functionNode.Name;
            IWorkflowFunctionInstance workflowFunctionInstance = instanceFactory.GetFunctionInstance(methodname);
            SomeDataStruct outputs = workflowFunctionInstance.Run(inputs);
            return outputs.Traverse(name);
        }

        private SomeDataStruct GetInputsForFunction(WorkflowActionNode function)
        {
            SomeDataStruct inputs = new SomeDataStruct();
            IEnumerable<WorkflowInputNode> inputNodes = function.Inputs;
            if (!inputNodes.Any())
                return inputs;

            foreach (WorkflowInputNode input in inputNodes)
                inputs[input.Name] = input.Source.GetValue(InputEvaluationContext);

            return inputs;
        }

        private SomeDataStruct GetInputsForMethod(WorkflowActionNode method)
        {
            SomeDataStruct inputs = new SomeDataStruct() ;
            IEnumerable<WorkflowInputNode> inputNodes = method.Inputs;
            if (!inputNodes.Any())
                return inputs;

            foreach (WorkflowInputNode input in inputNodes)
                inputs[input.Name] = input.Source.GetValue(InputEvaluationContext);

            return inputs;
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
                Output = workflowOutput
            };
        }
    }
}
