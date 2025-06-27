namespace WorkflowEngineIntegration
{
    public interface IBusinessLogicService
    {
        public Task Hello();
    }

    public class BusinessLogicService : IBusinessLogicService
    {
        public Task Hello()
        {
            Console.WriteLine("Hello there!");
            return Task.CompletedTask;
        }
    }
}
