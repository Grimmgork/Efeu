using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace Efeu.Runtime.Data
{
    public enum WorkflowTraversalNodeType
    {
        Leaf,
        Array,
        Struct
    }

    public interface IWorkflowTraversible
    {
        public WorkflowTraversalNodeType NodeType { get; }

        public IWorkflowTraversible GetField(string name);

        public IWorkflowTraversible GetIndex(int index);

        public SomeData Evaluate();
    }

    public static class WorkflowTraversableExtensions
    {
        public static SomeData Traverse(this IWorkflowTraversible start, DataTraversal traversal)
        {
            IWorkflowTraversible node = start;
            foreach (TraversalSegment segment in traversal)
            {
                if (node.NodeType != WorkflowTraversalNodeType.Struct)
                    throw new InvalidOperationException("Node is not a struct!");

                node = node.GetField(segment.Property);
                if (segment.IsIndex)
                {
                    if (node.NodeType != WorkflowTraversalNodeType.Array)
                        throw new InvalidOperationException("Node is not a array!");

                    node = node.GetIndex(segment.Index);
                }
            }
            return node.Evaluate();
        }
    }

    public struct SomeData : IWorkflowTraversible
    {
        private readonly object? scalarValue;
        private readonly IReadOnlyCollection<SomeData> arrayItems = ReadOnlyCollection<SomeData>.Empty;
        private readonly IReadOnlyDictionary<string, SomeData> structFields = ReadOnlyDictionary<string, SomeData>.Empty;

        public readonly WorkflowDataType DataType;

        public IReadOnlyCollection<SomeData> Items => arrayItems;
        public IReadOnlyDictionary<string, SomeData> Fields => structFields;
        public object? Value => scalarValue;
        public bool IsNull => DataType == WorkflowDataType.Null;
        public bool IsStruct => DataType == WorkflowDataType.Struct;
        public bool IsArray => DataType == WorkflowDataType.Array;
        public bool IsValue => DataType != WorkflowDataType.Struct && DataType != WorkflowDataType.Array;

        public WorkflowTraversalNodeType NodeType
        {
            get
            {
                if (IsStruct)
                    return WorkflowTraversalNodeType.Struct;
                if (IsArray)
                    return WorkflowTraversalNodeType.Array;
                else
                    return WorkflowTraversalNodeType.Leaf;
            }
        }

        public SomeData(IReadOnlyCollection<SomeData> items)
        {
            DataType = WorkflowDataType.Array;
            this.arrayItems = items;
        }

        public SomeData(IReadOnlyDictionary<string, SomeData> fields)
        {
            DataType = WorkflowDataType.Struct;
            this.structFields = fields;
        }

        public SomeData(IEnumerable<SomeData> items)
        {
            DataType = WorkflowDataType.Array;
            this.arrayItems = items.ToImmutableArray();
        }

        public SomeData(IEnumerable<KeyValuePair<string, SomeData>> fields)
        {
            DataType = WorkflowDataType.Struct;
            this.structFields = new Dictionary<string, SomeData>(fields);
        }

        private SomeData(WorkflowDataType type, object? value = null)
        {
            if (value is not null)
            {
                this.scalarValue = value;
                DataType = type;
            }
        }


        public SomeData(WorkflowDataType dataType, object? value, IReadOnlyCollection<SomeData> arrayItems, IReadOnlyDictionary<string, SomeData> structFields)
        {
            DataType = dataType;
            this.scalarValue = value;
            this.arrayItems = arrayItems;
            this.structFields = structFields;
        }


        public SomeData this[string name]
        {
            get
            {
                return structFields.GetValueOrDefault(name);
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

        public static SomeData Int32(Int32? value)
        {
            return new SomeData(WorkflowDataType.Int23, value);
        }

        public static SomeData Int64(Int64? value)
        {
            return new SomeData(WorkflowDataType.Int64, value);
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

        public static SomeData Timestamp(DateTimeOffset? value)
        {
            return new SomeData(WorkflowDataType.Timestamp, value);
        }

        public static SomeData Fork(int id)
        {
            return new SomeData(WorkflowDataType.Fork, id);
        }

        public static SomeData Reference<T>(T reference)
        {
            if (reference is null) return SomeData.Null();
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
                    UInt16  v => Int64(v),
                    UInt32  v => Int64(v),
                    Int16   v => Int32(v),
                    Int32   v => Int32(v),
                    Int64   v => Int64(v),
                    String  v => String(v),
                    Boolean v => Boolean(v),
                    Single  v => Single(v),
                    Double  v => Double(v),
                    Decimal v => Decimal(v),
                    _ => Reference(value)
                };
            }
        }

        public static SomeData Struct(IEnumerable<KeyValuePair<string, SomeData>> fields)
        {
            return new SomeData(fields);
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

        public static implicit operator SomeData(Int32 value) => SomeData.Int32(value);
        public static implicit operator SomeData(Int64 value) => SomeData.Int64(value);
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

        public DateTimeOffset ToDateTimeOffset()
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

        public bool Match<T>()
        {
            return Match<T>(out var _);
        }

        public bool Match<T>([NotNullWhen(true)] out T? obj)
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
                Dictionary<string, object?> fields = new Dictionary<string, object?>();
                foreach (KeyValuePair<string, SomeData> field in structFields!)
                {
                    fields.Add(field.Key, field.Value.ToPolymorphicObject());
                }
                return fields;
            }
            else
            {
                return scalarValue;
            }
        }

        public IWorkflowTraversible GetField(string name)
        {
            if (DataType == WorkflowDataType.Reference)
            {
                return TraverseFieldOfReference(name);
            }
            else
            {
                return this[name];
            }
        }

        private IWorkflowTraversible TraverseFieldOfReference(string name)
        {
            // TODO add IDynamicMetadataObject?
            if (scalarValue is IDictionary dictionary)
            {
                return SomeData.Parse(dictionary[name]);
            }
            else
            {
                return SomeData.Parse(scalarValue?.GetType().GetProperty(name)?.GetValue(scalarValue, null));
            }
        }

        public IWorkflowTraversible GetIndex(int index)
        {
            if (DataType == WorkflowDataType.Reference)
            {
                return TraverseIndexOfReference(index);
            }
            else
            {
                return this[index];
            }
        }

        private SomeData TraverseIndexOfReference(int index)
        {
            dynamic? obj = scalarValue;
            return SomeData.Parse(obj?[index]);
        }

        public SomeData Evaluate()
        {
            return this;
        }
    }
}
