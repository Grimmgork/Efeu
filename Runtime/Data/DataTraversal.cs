using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Efeu.Runtime.Data
{
    public struct DataTraversal : IEnumerable<TraversalSegment>
    {
        private readonly TraversalSegment[] segments;

        public int Length
        {
            get
            {
                return segments.Length;
            }
        }

        private static bool IsValidNameChar(char ch) => Char.IsAsciiLetterOrDigit(ch) || ch == '_' || ch == '-';

        public DataTraversal()
        {
            this.segments = [];
        }

        public DataTraversal(TraversalSegment[] segments)
        {
            this.segments = segments;
        }

        public static implicit operator DataTraversal(string value) => new DataTraversal(value);
        public static explicit operator string(DataTraversal value) => value.ToString();
        public static implicit operator DataTraversal(TraversalSegment[] segments) => new DataTraversal(segments);

        public DataTraversal(string path)
        {
            List<TraversalSegment> list = new List<TraversalSegment>();
            if (string.IsNullOrEmpty(path))
            {
                segments = [];
                return;
            }

            int index;
            TraversalSegment ParseName()
            {
                if (path[index] == '.')
                {
                    index++;
                }

                if (path[index] == '@')
                {
                    index++;
                }

                StringBuilder sb = new StringBuilder();
                for (; index < path.Length; index++)
                {
                    char c = path[index];
                    if (IsValidNameChar(path[index]))
                    {
                        sb.Append(path[index]);
                    }
                    else
                    {
                        if (path[index] == '.' || path[index] == '[')
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }

                return new TraversalSegment(sb.ToString());
            }

            TraversalSegment ParseIndex()
            {
                if (path[index] != '[')
                {
                    throw new Exception();
                }
                
                index++;
                StringBuilder sb = new StringBuilder();
                for (; index <= path.Length; index++)
                {
                    char c = path[index];
                    if (index == path.Length)
                    {
                        throw new Exception();
                    }

                    if (Char.IsAsciiDigit(path[index]))
                    {
                        sb.Append(path[index]);
                    }
                    else
                    {
                        if (path[index] == ']')
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }

                index++;
                return new TraversalSegment(Int32.Parse(sb.ToString()));
            }

            for (index = 0; index < path.Length;)
            {
                char c = path[index];
                if (path[index] == '[')
                {
                    list.Add(ParseIndex());
                }
                else if (path[index] == '.' || path[index] == '@' || IsValidNameChar(path[index]))
                {
                    list.Add(ParseName());
                }
                else
                {
                    throw new Exception();
                }

                if (index == path.Length)
                {
                    break;
                }
            }

            segments = list.ToArray();
        }

        public IEnumerator<TraversalSegment> GetEnumerator()
        {
            return segments?.AsEnumerable().GetEnumerator() ?? Enumerable.Empty<TraversalSegment>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (TraversalSegment segment in segments)
            {
                if (segment.IsIndex)
                {
                    sb.Append($"[{segment.Index}]");
                }
                else if (segment.IsProperty)
                {
                    if (!first)
                        sb.Append(".");
                    sb.Append(segment.Property);
                }
                first = false;
            }
            return sb.ToString();
        }
    }

    public struct TraversalSegment
    {
        public readonly string Property = "";
        public readonly int Index = -1;

        public bool IsProperty => !IsIndex;

        public bool IsIndex => Index >= 0;

        public TraversalSegment(int index)
        {
            Index = index;
        }

        public TraversalSegment(string name)
        {
            Property = name;
        }
    }
}
