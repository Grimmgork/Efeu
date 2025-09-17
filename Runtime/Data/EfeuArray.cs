using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuArray : EfeuObject, IEnumerable<EfeuValue>
    {
        public readonly IList<EfeuValue> Items = new List<EfeuValue>();

        public int Length => Items.Count;

        public EfeuValue this[int index]
        {
            get
            {
                return Items.ElementAtOrDefault(index);
            }
            set
            {
                Items[index] = value;
            }
        }

        public EfeuArray(IEnumerable<EfeuValue> items)
        {
            Items = new List<EfeuValue>(items);
        }

        public EfeuArray(params EfeuValue[] items)
        {
            Items = new List<EfeuValue>(items);
        }

        public EfeuArray()
        {

        }

        public override EfeuValue Call(int index)
        {
            return Items.ElementAtOrDefault(index);
        }

        public override void Call(int index, EfeuValue value)
        {
            Items[index] = value;
        }

        public override void Push(EfeuValue value)
        {
            Items.Add(value);
        }

        public override EfeuValue Shift()
        {
            if (Items.Count == 0)
            {
                return EfeuValue.Nil();
            }
            else
            {
                EfeuValue result = Items.FirstOrDefault();
                Items.RemoveAt(0);
                return result;
            }
        }

        public override EfeuValue Pop()
        {
            EfeuValue item = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
            return item;
        }

        public void Prepend(EfeuValue value)
        {
            Items.Prepend(value);
        }

        public override EfeuValue Clone()
        {
            return new EfeuArray();
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
