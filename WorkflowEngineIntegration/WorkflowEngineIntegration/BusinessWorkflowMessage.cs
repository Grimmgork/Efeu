using Workflows.Function;
using Workflows.Message;

namespace WorkflowEngineIntegration
{
    public class BusinessWorkflowMessage : WorkflowSignal
    {
        public BusinessWorkflowMessage(DateTime time) : base(time)
        {

        }
    }
}
