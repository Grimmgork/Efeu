using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuArray : EfeuObject, IReadOnlyList<EfeuValue>
    {
        public readonly IImmutableList<EfeuValue> Items = ImmutableList<EfeuValue>.Empty;

        public int Count => Items.Count;

        public static EfeuArray Empty = new EfeuArray();

        public static EfeuArray From(params EfeuValue[] items) => new EfeuArray(items);

        public EfeuValue this[int index]
        {
            get
            {
                return Items.ElementAtOrDefault(index);
            }
        }

        private EfeuArray(IImmutableList<EfeuValue> items)
        {
            Items = items;
        }

        public EfeuArray(params EfeuValue[] items)
        {
            Items = Items.AddRange(items);
        }

        public EfeuArray(IEnumerable<EfeuValue> items)
        {
            Items = Items.AddRange(items);
        }

        public EfeuArray Push(params EfeuValue[] items)
        {
            return new EfeuArray(Items.AddRange(items));
        }

        public EfeuArray Pop()
        {
            if (Items.Count == 0)
                return Empty;
            return new EfeuArray(Items.RemoveAt(Items.Count - 1));
        }

        public EfeuArray Unshift(params EfeuValue[] items)
        {
            return new EfeuArray(Items.InsertRange(0, items));
        }

        public EfeuArray Shift(int amount = 1)
        {
            return new EfeuArray(Items.Skip(amount));
        }

        public EfeuValue First()
        {
            return Items.First();
        }

        public EfeuValue Last()
        {
            return Items.Last();
        }

        public IEnumerator<EfeuValue> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
