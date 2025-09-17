using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public static class EfeuValueExtensions
    {
        public static EfeuValue Traverse(this EfeuValue start, DataTraversal traversal)
        {
            EfeuValue node = start;
            foreach (TraversalSegment segment in traversal)
            {
                if (segment.IsIndex)
                {
                    node = node.Call(segment.Index);
                }
                else
                {
                    node = node.Call(segment.Property);
                }
            }
            return node;
        }
    }
}
