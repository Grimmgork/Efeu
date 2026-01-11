using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuTime : EfeuObject
    {
        private readonly DateTimeOffset timestamp;

        public EfeuTime(long seconds, int milliseconds = 0)
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(seconds).AddMilliseconds(milliseconds);
        }

        public EfeuTime()
        {
            timestamp = DateTimeOffset.MinValue;
        }

        public EfeuTime(DateTime timestamp)
        {
            this.timestamp = timestamp;
        }

        public EfeuTime(DateTimeOffset timestamp)
        {
            this.timestamp = timestamp;
        }

        public static EfeuTime Now => new EfeuTime(DateTimeOffset.Now);

        public override long ToLong()
        {
            return timestamp.ToUnixTimeMilliseconds();
        }

        public override string ToString()
        {
            return timestamp.ToLocalTime().ToString();
        }

        public DateTimeOffset ToDateTimeOffset()
        {
            return timestamp;
        }

        public DateTime ToDateTime()
        {
            return timestamp.UtcDateTime;
        }
    }
}
