using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value;

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
        return Value.ToLocalTime().ToString(CultureInfo.InvariantCulture);
    }

    public override long AsLong()
    {
        return Value.ToUnixTimeMilliseconds();
    }

    public override bool AsBoolean()
    {
        return Value != DateTimeOffset.MinValue;
    }

    public override bool Equals(EfeuValue value)
    {
        if (value.AsObject() is EfeuTime time)
        {
            return this.Value == time.Value;
        }
        else
        {
            return Value.ToUnixTimeMilliseconds() == value.AsLong();
        }
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
