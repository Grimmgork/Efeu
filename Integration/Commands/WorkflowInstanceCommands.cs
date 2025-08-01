using Efeu.Integration.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Commands
{
    internal class WorkflowInstanceCommands : IWorkflowInstanceCommands
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWorkflowInstanceRepository instanceRepository;

        public WorkflowInstanceCommands(IUnitOfWork unitOfWork, IWorkflowInstanceRepository instanceRepository)
        {
            this.unitOfWork = unitOfWork;
            this.instanceRepository = instanceRepository;
        }

        public Task<int> Create(int worklflowDefinitionId)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> Execute(int workflowDefinitionId)
        {
            // run new workflow instance to completion or suspension
            throw new NotImplementedException();
        }

        public Task Continue(int workflowInstanceId)
        {
            // retry running a failed workflow
            throw new NotImplementedException();
        }

        public Task<int> Start(int workflowDefinitionId)
        {
            // dispatch a new workflow instance without waiting for completion
            throw new NotImplementedException();
        }

        public Task Step(int workflowInstanceId)
        {
            // run one step of a workflow instance
            throw new NotImplementedException();
        }

        public Task Resume(int workflowInstanceId)
        {
            // resume a paused workflow instance
            throw new NotImplementedException();
        }
    }
}
