using Efeu.Integration.Persistence;
using Efeu.Integration.Entities;
using Efeu.Integration.Model;
using Efeu.Runtime;
using Efeu.Runtime.Function;
using Efeu.Runtime.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class WorkflowInstanceCommands : IWorkflowInstanceCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWorkflowInstanceRepository instanceRepository;
        private readonly IWorkflowDefinitionRepository definitionRepository;
        private readonly IWorkflowDefinitionVersionRepository definitionVersionRepository;
        private readonly IWorkflowMethodProvider methodProvider;
        private readonly IWorkflowFunctionProvider functionProvider;
        private readonly IWorkflowTriggerProvider triggerProvider;

        public WorkflowInstanceCommands(IUnitOfWork unitOfWork, IWorkflowInstanceRepository instanceRepository, IWorkflowDefinitionRepository definitionRepository, IWorkflowDefinitionVersionRepository definitionVersionRepository, IWorkflowMethodProvider methodProvider, IWorkflowFunctionProvider functionProvider, IWorkflowTriggerProvider triggerProvider)
        {
            this.unitOfWork = unitOfWork;
            this.instanceRepository = instanceRepository;
            this.definitionRepository = definitionRepository;
            this.definitionVersionRepository = definitionVersionRepository;
            this.methodProvider = methodProvider;
            this.functionProvider = functionProvider;
            this.triggerProvider = triggerProvider;
        }

        public async Task<WorkflowExecutionResult> ExecuteAsync(int workflowDefinitionId, CancellationToken token)
        {
            WorkflowDefinitionVersionEntity definitionVersion = await definitionVersionRepository.GetLatestVersion(workflowDefinitionId);

            WorkflowRuntime instance = new WorkflowRuntime(definitionVersion.Definition, methodProvider, functionProvider, triggerProvider);
            WorkflowRuntimeExport instanceExport = instance.Export();

            WorkflowInstanceEntity instanceEntity = new WorkflowInstanceEntity()
            {
                WorkflowDefinitionVersionId = definitionVersion.Id,
                CurrentMethodId = instanceExport.CurrentMethodId,
                Input = instanceExport.Input,
                ExecutionState = WorkflowExecutionState.Running,
                IsProcessing = true
            };

            int instanceId = await instanceRepository.Add(instanceEntity);
            instanceEntity.Id = instanceId;

            Exception? exception = null;
            try
            {
                await instance.RunAsync(token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            instanceExport = instance.Export();

            WorkflowExecutionState executionState = exception is not null ? WorkflowExecutionState.Failed : instance.State switch
            {
                WorkflowRuntimeState.Suspended => WorkflowExecutionState.Suspended,
                WorkflowRuntimeState.Done => WorkflowExecutionState.Completed,
                _ => throw new Exception($"Invalid state {instance.State}")
            };

            instanceEntity.Input = instanceExport.Input;
            instanceEntity.Output = instanceExport.Output;
            instanceEntity.ReturnStack = instanceExport.ReturnStack;
            instanceEntity.State = instanceExport.State;
            instanceEntity.MethodData = instanceExport.MethodData;
            instanceEntity.MethodOutput = instanceExport.MethodOutput;
            instanceEntity.ExecutionState = executionState;

            await instanceRepository.Update(instanceEntity);

            return new WorkflowExecutionResult()
            {
                 Output = instanceExport.Output,
                 ExecutionState = executionState,
                 Exception = exception
            };
        }

        public async Task<WorkflowExecutionResult> StepAsync(int id)
        {
            WorkflowInstanceEntity? instanceEntity = await instanceRepository.GetForProcessing(id, [WorkflowExecutionState.Paused, WorkflowExecutionState.Failed], WorkflowExecutionState.Running);
            if (instanceEntity == null)
            {
                throw new Exception("Instance is not in the correct state!");
            }

            WorkflowDefinitionVersionEntity definitionVersion = await definitionVersionRepository.GetByIdAsync(instanceEntity.WorkflowDefinitionVersionId);
            WorkflowRuntimeExport instanceExport = new WorkflowRuntimeExport()
            {
                Input = instanceEntity.Input,
                Output = instanceEntity.Output,
                ReturnStack = instanceEntity.ReturnStack,
                State = WorkflowRuntimeState.Running,
                MethodData = instanceEntity.MethodData,
                MethodOutput = instanceEntity.MethodOutput
            };

            WorkflowRuntime instance = WorkflowRuntime.Import(instanceExport, definitionVersion.Definition, methodProvider, functionProvider, triggerProvider);
            Exception? exception = null;
            try
            {
                await instance.StepAsync();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            instanceExport = instance.Export();

            WorkflowExecutionState executionState = exception is not null ? WorkflowExecutionState.Failed : instance.State switch
            {
                WorkflowRuntimeState.Suspended => WorkflowExecutionState.Suspended,
                WorkflowRuntimeState.Done => WorkflowExecutionState.Completed,
                WorkflowRuntimeState.Running => WorkflowExecutionState.Paused,
                _ => throw new Exception($"Invalid state {instance.State}")
            };

            instanceEntity.Input = instanceExport.Input;
            instanceEntity.Output = instanceExport.Output;
            instanceEntity.ReturnStack = instanceExport.ReturnStack;
            instanceEntity.State = instanceExport.State;
            instanceEntity.MethodData = instanceExport.MethodData;
            instanceEntity.MethodOutput = instanceExport.MethodOutput;
            instanceEntity.ExecutionState = executionState;

            await instanceRepository.Update(instanceEntity);

            return new WorkflowExecutionResult()
            {
                Output = instanceExport.Output,
                ExecutionState = executionState,
                Exception = exception
            };
        }

        public async Task<WorkflowExecutionResult> ContinueAsync(int id)
        {
            WorkflowInstanceEntity? instanceEntity = await instanceRepository.GetForProcessing(id, [WorkflowExecutionState.Paused, WorkflowExecutionState.Failed], WorkflowExecutionState.Running);
            if (instanceEntity == null)
            {
                throw new Exception("Instance is not in the correct state!");
            }

            WorkflowDefinitionVersionEntity definitionVersion = await definitionVersionRepository.GetByIdAsync(instanceEntity.WorkflowDefinitionVersionId);
            WorkflowRuntimeExport instanceExport = new WorkflowRuntimeExport()
            {
                Input = instanceEntity.Input,
                Output = instanceEntity.Output,
                ReturnStack = instanceEntity.ReturnStack,
                State = WorkflowRuntimeState.Running,
                MethodData = instanceEntity.MethodData,
                MethodOutput = instanceEntity.MethodOutput
            };

            WorkflowRuntime instance = WorkflowRuntime.Import(instanceExport, definitionVersion.Definition, methodProvider, functionProvider, triggerProvider);
            Exception? exception = null;
            try
            {
                await instance.StepAsync();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            instanceExport = instance.Export();

            WorkflowExecutionState executionState = exception is not null ? WorkflowExecutionState.Failed : instance.State switch
            {
                WorkflowRuntimeState.Suspended => WorkflowExecutionState.Suspended,
                WorkflowRuntimeState.Done => WorkflowExecutionState.Completed,
                WorkflowRuntimeState.Running => WorkflowExecutionState.Running,
                _ => throw new Exception($"Invalid state {instance.State}")
            };

            instanceEntity.Input = instanceExport.Input;
            instanceEntity.Output = instanceExport.Output;
            instanceEntity.ReturnStack = instanceExport.ReturnStack;
            instanceEntity.State = instanceExport.State;
            instanceEntity.MethodData = instanceExport.MethodData;
            instanceEntity.MethodOutput = instanceExport.MethodOutput;
            instanceEntity.ExecutionState = executionState;

            await instanceRepository.Update(instanceEntity);

            return new WorkflowExecutionResult()
            {
                Output = instanceExport.Output,
                ExecutionState = executionState,
                Exception = exception
            };
        }

        public async Task<WorkflowExecutionResult> SendSignalAsync(int id, object signal)
        {
            // TODO send multiple signals if multiple triggers match

            WorkflowInstanceEntity? instanceEntity = await instanceRepository.GetForProcessing(id, [WorkflowExecutionState.Suspended], WorkflowExecutionState.Running);
            if (instanceEntity == null)
            {
                throw new Exception("Instance is not in the correct state!");
            }

            WorkflowDefinitionVersionEntity definitionVersion = await definitionVersionRepository.GetByIdAsync(instanceEntity.WorkflowDefinitionVersionId);
            WorkflowRuntimeExport instanceExport = new WorkflowRuntimeExport()
            {
                Input = instanceEntity.Input,
                Output = instanceEntity.Output,
                ReturnStack = instanceEntity.ReturnStack,
                State = WorkflowRuntimeState.Suspended,
                MethodData = instanceEntity.MethodData,
                MethodOutput = instanceEntity.MethodOutput
            };

            WorkflowRuntime instance = WorkflowRuntime.Import(instanceExport, definitionVersion.Definition, methodProvider, functionProvider, triggerProvider);
            Exception? exception = null;
            try
            {
                instance.Signal(default, signal);
                await instance.RunAsync();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            instanceExport = instance.Export();

            WorkflowExecutionState executionState = exception is not null ? WorkflowExecutionState.Failed : instance.State switch
            {
                WorkflowRuntimeState.Suspended => WorkflowExecutionState.Suspended,
                WorkflowRuntimeState.Done => WorkflowExecutionState.Completed,
                _ => throw new Exception($"Invalid state {instance.State}")
            };

            instanceEntity.Input = instanceExport.Input;
            instanceEntity.Output = instanceExport.Output;
            instanceEntity.ReturnStack = instanceExport.ReturnStack;
            instanceEntity.State = instanceExport.State;
            instanceEntity.MethodData = instanceExport.MethodData;
            instanceEntity.MethodOutput = instanceExport.MethodOutput;
            instanceEntity.ExecutionState = executionState;

            await instanceRepository.Update(instanceEntity);

            return new WorkflowExecutionResult()
            {
                Output = instanceExport.Output,
                ExecutionState = executionState,
                Exception = exception
            };
        }

        public async Task AbortAsync(int id)
        {
            WorkflowInstanceEntity? instance = await instanceRepository.GetForProcessing(id, [WorkflowExecutionState.Paused, WorkflowExecutionState.Suspended, WorkflowExecutionState.Failed], WorkflowExecutionState.Aborted);
            if (instance == null)
            {
                throw new Exception("Instance is not in the correct state!");
            }
        }

        public async Task DeleteAsync(int id)
        {
            bool result = await instanceRepository.Delete(id, [WorkflowExecutionState.Aborted, WorkflowExecutionState.Completed]);
            if (!result)
            {
                throw new Exception("Instance is not in the correct state!");
            }
        }
    }
}
