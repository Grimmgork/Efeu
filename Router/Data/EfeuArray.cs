using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Data
{
    public class EfeuArray : EfeuObject, IEnumerable<EfeuValue>
    {
        public readonly IImmutableList<EfeuValue> Items = ImmutableList<EfeuValue>.Empty;

        public EfeuValue this[int index]
        {
            get
            {
                return Items.ElementAtOrDefault(index);
            }
        }

        public EfeuArray(IEnumerable<EfeuValue> items)
        {
            Items = Items.AddRange(items);
        }

        public EfeuArray(params EfeuValue[] items)
        {
            Items = Items.AddRange(items);
        }

        public override IEnumerable<EfeuValue> Each()
        {
            return Items;
        }

        public override int Length()
        {
            return Items.Count;
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
