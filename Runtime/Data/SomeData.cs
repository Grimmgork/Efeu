using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;

namespace Efeu.Runtime.Data
{
    public class SomeStruct : Dictionary<string, SomeData>, ISomeTraversableData
    {
        public new SomeData this[string name]
        {
            get
            {
                return this.GetValueOrDefault(name);
            }
            set
            {
                base[name] = value;
            }
        }

        public TraversalNodeType TraversalNodeType => TraversalNodeType.Struct;

        public SomeData TraversalEnd()
        {
            return SomeData.Struct(this);
        }

        public SomeData TraverseByIndex(int index)
        {
            throw new InvalidOperationException();
        }

        public SomeData TraverseByName(string name)
        {
            return this[name];
        }
    }

    public enum TraversalNodeType
    {
        Array,
        Struct,
        Leaf
    }

    public interface ISomeTraversableData
    {
        public TraversalNodeType TraversalNodeType { get; }

        public SomeData TraverseByName(string name);

        public SomeData TraverseByIndex(int index);

        public SomeData TraversalEnd();
    }

    public static class SomeTraversableDataExtensions
    {
        public static SomeData Traverse(this ISomeTraversableData root, DataTraversal traversal)
        {
            ISomeTraversableData node = root;
            foreach (TraversalSegment segment in traversal)
            {
                if (node.TraversalNodeType != TraversalNodeType.Struct)
                    throw new InvalidOperationException("Node is not a struct!");

                node = node.TraverseByName(segment.Property);
                if (segment.IsIndex)
                {
                    if (node.TraversalNodeType != TraversalNodeType.Array)
                        throw new InvalidOperationException("Node is not a array!");

                    node = node.TraverseByIndex(segment.Index);
                }
            }
            return node.TraversalEnd();
        }
    }

    public struct SomeData : ISomeTraversableData
    {
        private readonly object? scalarValue;
        private readonly IReadOnlyCollection<SomeData> arrayItems = ReadOnlyCollection<SomeData>.Empty;
        private readonly IReadOnlyDictionary<string, SomeData> structProperties = ReadOnlyDictionary<string, SomeData>.Empty;

        public readonly WorkflowDataType DataType;

        public IReadOnlyCollection<SomeData> Items => arrayItems;
        public IReadOnlyDictionary<string, SomeData> Properties => structProperties;
        public object? Value => scalarValue;
        public bool IsNull => DataType == WorkflowDataType.Null;
        public bool IsStruct => DataType == WorkflowDataType.Struct;
        public bool IsArray => DataType == WorkflowDataType.Array;
        public bool IsScalar => DataType != WorkflowDataType.Struct && DataType != WorkflowDataType.Array;
        public bool IsReference => DataType == WorkflowDataType.Reference;

        public TraversalNodeType TraversalNodeType 
        {
            get
            {
                if (IsStruct)
                    return TraversalNodeType.Struct;
                if (IsArray)
                    return TraversalNodeType.Array;
                else
                    return TraversalNodeType.Leaf;
            }
        }

        public SomeData(IReadOnlyDictionary<string, SomeData> structure)
        {
            DataType = WorkflowDataType.Struct;
            this.structProperties = structure;
        }

        public SomeData(IEnumerable<SomeData> items)
        {
            DataType = WorkflowDataType.Array;
            this.arrayItems = items.ToImmutableArray();
        }

        public SomeData(IEnumerable<KeyValuePair<string, SomeData>> properties)
        {
            DataType = WorkflowDataType.Struct;
            this.structProperties = new Dictionary<string, SomeData>(properties);
        }

        public SomeData(WorkflowDataType type, object? value = null)
        {
            if (value is not null)
            {
                this.scalarValue = value;
                DataType = type;
            }
        }

        public SomeData this[string name]
        {
            get
            {
                return structProperties.GetValueOrDefault(name);
            }
        }

        public SomeData this[int index]
        {
            get
            {
                return arrayItems.ElementAtOrDefault(index);
            }
        }

        public static SomeData Null()
        {
            return new SomeData();
        }

        public static SomeData Integer(Int32? value)
        {
            return new SomeData(WorkflowDataType.Integer, value);
        }

        public static SomeData Long(Int64? value)
        {
            return new SomeData(WorkflowDataType.Long, value);
        }

        public static SomeData Boolean(bool? value)
        {
            return new SomeData(WorkflowDataType.Boolean, value);
        }

        public static SomeData String(string? value)
        {
            return new SomeData(WorkflowDataType.String, value);
        }

        public static SomeData Single(Single? value)
        {
            return new SomeData(WorkflowDataType.Single, value);
        }

        public static SomeData Double(Double? value)
        {
            return new SomeData(WorkflowDataType.Single, value);
        }

        public static SomeData Decimal(Decimal? value)
        {
            return new SomeData(WorkflowDataType.Decimal, value);
        }

        public static SomeData Timestamp(DateTime? value)
        {
            return new SomeData(WorkflowDataType.Timestamp, value);
        }

        public static SomeData Reference<T>(T reference) where T : class
        {
            return new SomeData(WorkflowDataType.Reference, reference);
        }

        public static SomeData Parse(object? value)
        {
            if (value == null)
            {
                return SomeData.Null();
            }
            else if(value is SomeData someData)
            {
                return someData;
            }
            else if (value is IDictionary properties)
            {
                return SomeData.Struct(
                     properties.Cast<DictionaryEntry>().Select(p => 
                        new KeyValuePair<string, SomeData>((string)p.Key, SomeData.Parse(p.Value))));
            }
            else if (value is IEnumerable items and not string)
            {
                return SomeData.Array(items.Cast<object>().Select(i => 
                    SomeData.Parse(i)));
            }
            else
            {
                return value switch
                {
                    UInt16  v => Long(v),
                    UInt32  v => Long(v),
                    Int16   v => Integer(v),
                    Int32   v => Integer(v),
                    Int64   v => Long(v),
                    String  v => String(v),
                    Boolean v => Boolean(v),
                    Single  v => Single(v),
                    Double  v => Double(v),
                    Decimal v => Decimal(v),
                    _ => throw new Exception($"Cannot convert {value.GetType()} into {nameof(SomeData)}")
                };
            }
        }

        public static SomeData Struct(IEnumerable<KeyValuePair<string, SomeData>> properties)
        {
            return new SomeData(properties);
        }

        public static SomeData Struct(IDictionary<string, SomeData> structure)
        {
            return new SomeData(structure);
        }

        public static SomeData Struct()
        {
            return new SomeData(Enumerable.Empty<KeyValuePair<string, SomeData>>());
        }

        public static SomeData Array(params SomeData[] items)
        {
            return new SomeData(items);
        }

        public static SomeData Array(IList<SomeData> items)
        {
            return new SomeData(items);
        }

        public static SomeData Array(IEnumerable<SomeData> items)
        {
            return new SomeData(items);
        }

        public static explicit operator Int32(SomeData value) => value.ToInt32();
        public static explicit operator String(SomeData value) => value.ToString();
        public static explicit operator Boolean(SomeData value) => value.ToBoolean();

        public static implicit operator SomeData(Int32 value) => SomeData.Integer(value);
        public static implicit operator SomeData(Int64 value) => SomeData.Long(value);
        public static implicit operator SomeData(String value) => SomeData.String(value);
        public static implicit operator SomeData(Boolean value) => SomeData.Boolean(value);
        public static implicit operator SomeData(Single value) => SomeData.Single(value);
        public static implicit operator SomeData(Double value) => SomeData.Double(value);
        public static implicit operator SomeData(Decimal value) => SomeData.Decimal(value);

        public Int16 ToInt16()
        {
            return Convert.ToInt16(scalarValue);
        }

        public Int32 ToInt32()
        {
            return Convert.ToInt32(scalarValue);
        }

        public Int64 ToInt64()
        {
            return Convert.ToInt64(scalarValue);
        }

        public Single ToSingle()
        {
            return Convert.ToSingle(scalarValue);
        }

        public Single ToDouble()
        {
            return Convert.ToSingle(scalarValue);
        }

        public Boolean ToBoolean()
        {
            return Convert.ToBoolean(scalarValue);
        }

        public Decimal ToDecimal()
        {
            return Convert.ToDecimal(scalarValue);
        }

        public DateTime ToDateTime()
        {
            return Convert.ToDateTime(scalarValue);
        }

        public Int16? ToInt16Nullable()
        {
            if (IsNull) return null;
            return Convert.ToInt16(scalarValue);
        }

        public Int32? ToInt32Nullable()
        {
            if (IsNull) return null;
            return Convert.ToInt32(scalarValue);
        }

        public Int64? ToInt64Nullable()
        {
            if (IsNull) return null;
            return Convert.ToInt64(scalarValue);
        }

        public string? ToStringNullable()
        {
            if (IsNull) return null;
            return Convert.ToString(scalarValue);
        }

        public bool MatchReference<T>([NotNullWhen(true)] out T? obj)
        {
            if (scalarValue?.GetType().IsAssignableTo(typeof(T)) ?? false)
            {
                obj = (T)scalarValue;
                return true;
            }
            else
            {
                obj = default;
                return false;
            }
        }

        public new string ToString()
        {
            if (IsNull)
                return "NULL";

            if (IsArray)
                return "ARRAY";

            if (IsStruct)
                return "STRUCT";

            return Convert.ToString(scalarValue) ?? "";
        }

        public object? ToPolymorphicObject()
        {
            if (IsNull)
            {
                return null;
            }
            else if (IsArray)
            {
                List<object?> items = new List<object?>();
                foreach (SomeData item in arrayItems!)
                {
                    items.Add(item.ToPolymorphicObject());
                }
                return items;
            }
            else if (IsStruct)
            {
                Dictionary<string, object?> properties = new Dictionary<string, object?>();
                foreach (KeyValuePair<string, SomeData> property in structProperties!)
                {
                    properties.Add(property.Key, property.Value.ToPolymorphicObject());
                }
                return properties;
            }
            else
            {
                return scalarValue;
            }
        }

        public SomeData TraverseByName(string name)
        {
            return this[name];
        }

        public SomeData TraverseByIndex(int index)
        {
            return this[index];
        }

        public SomeData TraversalEnd()
        {
            return this;
        }
    }
}
