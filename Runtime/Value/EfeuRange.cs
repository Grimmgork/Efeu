using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value;

public class EfeuRange : EfeuObject, IEnumerable<EfeuValue>
{
    public readonly int Start;
    public readonly int End;

    public EfeuRange(int start, int end)
    {
        this.Start = start;
        this.End = end;
    }

    public IEnumerator<EfeuValue> GetEnumerator()
    {
        return Enumerable.Range(Start, End)
            .Select(i => (EfeuValue)i)
            .GetEnumerator();
    }

    public override bool AsBoolean()
    {
        return this.Any();
    }

    public override bool Equals(EfeuValue value)
    {
        if (value.AsObject() is EfeuRange range)
        {
            return range.Start == this.Start && range.End == this.End;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Start, this.End);
    }

    public override string ToString()
    {
        return $"[{this.Start}..{this.End}]";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
