﻿using System;
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
    public struct SomeDataTraversal : IEnumerable<SomeDataTraversalSegment>
    {
        private static Regex regex = new Regex("([a-zA-Z_1-9-]+)(?:\\[([1-9]+)\\])?");
        private readonly SomeDataTraversalSegment[] segments;
        private readonly string path;

        public int Length
        {
            get
            {
                return segments?.Length ?? 0;
            }
        }

        public SomeDataTraversal()
        {
            segments = [];
            path = "";
        }

        public SomeDataTraversal(string path)
        {
            segments = [];
            this.path = path;
            if (string.IsNullOrEmpty(path))
                return;

            string[] stringSegments = path.Split(".");
            segments = new SomeDataTraversalSegment[stringSegments.Length];
            int i = 0;
            foreach (string segment in stringSegments)
            {
                bool isLast = i == stringSegments.Length-1;
                Match match = regex.Match(segment);
                if (match.Success)
                {
                    if (match.Groups.Count < 2)
                    {
                        int index = Convert.ToInt32(match.Groups[2].Value);
                        segments[i] = new SomeDataTraversalSegment(match.Groups[1].Value, index, isLast);
                    }
                    else
                    {
                        segments[i] = new SomeDataTraversalSegment(match.Groups[1].Value, isLast);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid segment '{segment}'.");
                }

                i++;
            }
        }

        public static implicit operator SomeDataTraversal(string value) => new SomeDataTraversal(value);
        public static explicit operator string(SomeDataTraversal value) => value.ToString();

        public IEnumerator<SomeDataTraversalSegment> GetEnumerator()
        {
            return segments?.AsEnumerable().GetEnumerator() ?? Enumerable.Empty<SomeDataTraversalSegment>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            return path ?? "";
        }
    }

    public struct SomeDataTraversalSegment
    {
        public readonly string Name = "";
        public readonly int Indexer = -1;
        public readonly bool IsLast;
        public bool HasIndexer => Indexer >= 0;

        public SomeDataTraversalSegment(string segment, int indexer, bool isLast)
        {
            Name = segment;
            Indexer = indexer;
            IsLast = isLast;
        }

        public SomeDataTraversalSegment(string segment, bool isLast)
        {
            Name = segment;
            IsLast = isLast;
        }
    }
}
