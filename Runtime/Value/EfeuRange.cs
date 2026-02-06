using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
