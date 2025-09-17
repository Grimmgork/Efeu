using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuTime : EfeuObject
    {
        public readonly DateTimeOffset Timestamp;

        public EfeuTime(long seconds, int milliseconds = 0)
        {
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(seconds).AddMilliseconds(milliseconds);
        }

        public EfeuTime()
        {
            Timestamp = DateTimeOffset.MinValue;
        }

        public EfeuTime(DateTime dt)
        {
            Timestamp = dt;
        }

        public EfeuTime(DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
        }

        public static EfeuTime Now => new EfeuTime(DateTimeOffset.Now);

        public override long ToLong()
        {
            return Timestamp.ToUnixTimeSeconds();
        }

        public override string ToString()
        {
            return Timestamp.ToLocalTime().ToString();
        }
    }
}
