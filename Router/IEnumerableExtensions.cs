using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Router
{
    public static class IEnumerableExtensions
    {
        public static (IEnumerable<T> Matches, IEnumerable<T> NonMatches) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            List<T> matches;
            List<T> nonMatches;

            if (source is ICollection<T> col)
            {
                // Preallocate half as a heuristic; avoids constant resizes
                int capacity = col.Count / 2 + 1;
                matches = new(capacity);
                nonMatches = new(capacity);
            }
            else
            {
                matches = new();
                nonMatches = new();
            }

            foreach (var item in source)
            {
                if (predicate(item))
                    matches.Add(item);
                else
                    nonMatches.Add(item);
            }

            return (matches, nonMatches);
        }
    }
}
