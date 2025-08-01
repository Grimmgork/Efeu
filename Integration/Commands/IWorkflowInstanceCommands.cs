using Efeu.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    public interface IWorkflowInstanceCommands
    {
        public Task<int> Create(int worklflowDefinitionId);

        public Task<int> Execute(int workflowDefinitionId);

        public Task<int> Start(int workflowDefinitionId);

        /// <summary>
        /// Continue from breakpoint
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        public Task Continue(int workflowInstanceId);

        /// <summary>
        /// Step breakpoint forward
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        public Task Step(int workflowInstanceId);

        /// <summary>
        /// Send a trigger to all suspended workflows
        /// Triggers are queued and timestamp is validated
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public Task SendSignal(object signal);

        /// <summary>
        /// Abort a workflow wich is in a breakpoint, suspended
        /// </summary>
        /// <param name="workflowInstanceId"></param>
        /// <returns></returns>
        public Task Abort(int workflowInstanceId);

        /// <summary>
        /// Delete a aborted or completed workflow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task Delete(int id);
    }
}
