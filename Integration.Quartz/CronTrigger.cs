using Efeu.Integration.Foreign;

namespace Efeu.Integration.Quartz
{
    public class CronTrigger : IEfeuTrigger
    {
        public Task AttachAsync(EfeuTriggerContext context, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task DetatchAsync(EfeuTriggerContext context)
        {
            throw new NotImplementedException();
            // since this trigger is working in memory it must not propagate exceptions to prevent the outer transaction from completing
        }
    }
}
