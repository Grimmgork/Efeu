using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuTime : EfeuObject
    {
        public readonly DateTimeOffset Value;

        public EfeuTime(long seconds, int milliseconds = 0)
        {
            Value = DateTimeOffset.FromUnixTimeSeconds(seconds).AddMilliseconds(milliseconds);
        }

        public EfeuTime()
        {
            Value = DateTimeOffset.MinValue;
        }

        public EfeuTime(DateTime timestamp)
        {
            this.Value = timestamp;
        }

        public EfeuTime(DateTimeOffset timestamp)
        {
            this.Value = timestamp;
        }

        public static EfeuTime Now => new EfeuTime(DateTimeOffset.Now);

        public override string ToString()
        {
            return Value.ToLocalTime().ToString();
        }
    }
}
