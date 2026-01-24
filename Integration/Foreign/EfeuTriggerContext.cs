using Efeu.Runtime;
using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration.Foreign
{
    public class EfeuTriggerContext
    {
        public readonly Guid Id;

        public readonly EfeuValue Input;

        public readonly DateTimeOffset Timestamp;

        public Task ModifyAsync(string name, EfeuMessageTag tag) => Task.CompletedTask;

        public Task ScheduleAsync(DateTimeOffset time) => Task.CompletedTask;
    }
}
