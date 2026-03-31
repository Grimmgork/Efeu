using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Integration
{
    internal static class CollectionExtentions
    {
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            foreach (T item in collection)
            {
                if (predicate(item))
                {
                    collection.Remove(item);
                }
            }
        }
    }
}
