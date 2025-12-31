using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    public static class TaskExtensions
    {
        public static async Task RetryWhile(this Task task, Func<Exception, int, DateTimeOffset, bool> until) 
        {
            int times = 0;
            DateTimeOffset startTime = DateTimeOffset.UtcNow;
            Exception exception = new Exception();
            do
            {
                try
                {
                    await task;
                    return;
                }
                catch (Exception ex)
                {
                    times++;
                    exception = ex;
                }
            }
            while (until(exception, times, startTime));
            throw exception;
        }
    }
}
