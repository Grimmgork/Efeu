using Efeu.Integration.Model;
using Efeu.Runtime;
using Efeu.Runtime.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IWorkflowInstanceCommands
    {
        /// <summary>
        /// Start a new workflow and run it to suspension
        /// </summary>
        /// <param name="workflowDefinitionId"></param>
        /// <returns></returns>
        public Task<WorkflowExecutionResult> ExecuteAsync(int workflowDefinitionId, CancellationToken token);

        /// <summary>
        /// Continue a paused or failed workflow and run to suspension
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        public Task<WorkflowExecutionResult> ContinueAsync(int workflowInstanceId);

        /// <summary>
        /// Step a paused workflow forward
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        public Task<WorkflowExecutionResult> StepAsync(int workflowInstanceId);

        /// <summary>
        /// Send and process a signal to a workflow and run to suspension
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public Task<WorkflowExecutionResult> SendSignalAsync(int workflowInstanceId, object signal);

        /// <summary>
        /// Abort a workflow wich is paused, suspended, failed
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        public Task AbortAsync(int workflowInstanceId);

        /// <summary>
        /// Delete a aborted, completed workflow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task DeleteAsync(int id);
    }
}
