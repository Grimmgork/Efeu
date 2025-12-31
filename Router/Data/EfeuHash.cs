using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router.Data
{
    public class EfeuHash : EfeuObject, IEnumerable<KeyValuePair<string, EfeuValue>>
    {
        public readonly IImmutableDictionary<string, EfeuValue> Hash = ImmutableDictionary<string, EfeuValue>.Empty;

        public EfeuHash(IEnumerable<KeyValuePair<string, EfeuValue>> fields)
        {
            Hash = Hash.AddRange(fields);
        }

        public EfeuHash(params KeyValuePair<string, EfeuValue>[] fields)
        {
            Hash = Hash.AddRange(fields);
        }

        public EfeuValue this[string field]
        {
            get
            {
                return Hash.GetValueOrDefault(field, default);
            }
        }

        public override EfeuValue Call(EfeuValue field)
        {
            return Hash.GetValueOrDefault(field.ToString(), EfeuValue.Nil());
        }

        public override EfeuValue Call(EfeuValue field, EfeuValue value)
        {
            return new EfeuHash(Hash.SetItem(field.ToString(), value));
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
    }
}
