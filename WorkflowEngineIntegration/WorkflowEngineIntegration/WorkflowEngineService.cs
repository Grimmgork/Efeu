
using Workflows;

namespace WorkflowEngineIntegration
{
    public interface IWorkflowEngineService
    {
        int StartWorkflow(string name);

        void PauseWorkflow(int instanceId);

        void CancelWorfklow(int instanceId);
    }

    public class WorkflowEngineService : IWorkflowEngineService, IHostedService
    {
        private IWorkflowActionInstanceFactory instanceFactory;
        private IWorkflowSignalHandler messageHandler;

        public WorkflowEngineService(IWorkflowActionInstanceFactory instanceFactory, IWorkflowSignalHandler messageHandler)
        {
            this.instanceFactory = instanceFactory;
            this.messageHandler = messageHandler;
        }

        public void CancelWorfklow(int instanceId)
        {
            throw new NotImplementedException();
        }

        public void PauseWorkflow(int instanceId)
        {
            throw new NotImplementedException();
        }

        public int StartWorkflow(string name)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // load from disk
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // save to disk
            return Task.CompletedTask;
        }
    }
}
