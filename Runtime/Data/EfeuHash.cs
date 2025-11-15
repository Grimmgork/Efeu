using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public class EfeuHash : EfeuObject, IEnumerable<KeyValuePair<string, EfeuValue>>, IEnumerable<EfeuValue>
    {
        public readonly IDictionary<string, EfeuValue> Hash = new Dictionary<string, EfeuValue>();

        public EfeuHash(IEnumerable<KeyValuePair<string, EfeuValue>> fields)
        {
            Hash = new Dictionary<string, EfeuValue>(fields);
        }

        public EfeuHash(params KeyValuePair<string, EfeuValue>[] fields)
        {
            Hash = new Dictionary<string, EfeuValue>(fields);
        }

        public EfeuValue this[string field]
        {
            get
            {
                return Hash.GetValueOrDefault(field, default);
            }
            set
            {
                Hash[field] = value;
            }
        }

        public override EfeuValue Call(string field)
        {
            return Hash.GetValueOrDefault(field, EfeuValue.Nil());
        }

        public override void Call(string field, EfeuValue value)
        {
            Hash[field] = value;
        }

        public override IEnumerable<KeyValuePair<string, EfeuValue>> Fields()
        {
            return Hash;
        }

        public override IEnumerable<EfeuValue> Each()
        {
            return Hash.Select(x => (EfeuValue)new EfeuArray([
                x.Key, x.Value
            ]));
        }

        public IEnumerator<KeyValuePair<string, EfeuValue>> GetEnumerator()
        {
            return Hash.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<EfeuValue> IEnumerable<EfeuValue>.GetEnumerator()
        {
            return Each().GetEnumerator();
        }
    }
}
