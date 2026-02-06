using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public class EfeuHash : EfeuObject, IEnumerable<KeyValuePair<string, EfeuValue>>
    {
        public readonly IImmutableDictionary<string, EfeuValue> Hash = ImmutableDictionary<string, EfeuValue>.Empty;

        public static EfeuHash Empty = new EfeuHash();

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

        public EfeuHash With(string key, EfeuValue value)
        {
            return new EfeuHash(Hash.SetItem(key, value));
        }

        public EfeuHash Remove(string key)
        {
            return new EfeuHash(Hash.Remove(key));
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
