using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Function;

namespace Efeu.Runtime.Data
{
    public class SomeDataStruct : Dictionary<string, SomeData>, ISomeTraversableData
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

        public SomeData ResolveTraversal()
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

        public SomeData ResolveTraversal();
    }

    public static class SomeTraversableDataExtensions
    {
        public static SomeData Traverse(this ISomeTraversableData root, SomeDataTraversal traversal)
        {
            ISomeTraversableData node = root;
            foreach (SomeDataTraversalSegment segment in traversal)
            {
                if (node.TraversalNodeType != TraversalNodeType.Struct)
                    throw new InvalidOperationException("Node is not a struct!");

                node = node.TraverseByName(segment.Name);
                if (segment.HasIndexer)
                {
                    if (node.TraversalNodeType != TraversalNodeType.Array)
                        throw new InvalidOperationException("Node is not a array!");

                    node = node.TraverseByIndex(segment.Indexer);
                }
            }
            return node.ResolveTraversal();
        }
    }

    public struct SomeData : ISomeTraversableData
    {
        public readonly WorkflowDataType DataType;

        private readonly object? scalarValue;
        private readonly IList<SomeData>? arrayItems;
        private readonly IDictionary<string, SomeData>? structProperties;

        public IList<SomeData> Items => arrayItems ?? throw new InvalidOperationException("Not an Array.");

        public IDictionary<string, SomeData> Properties => structProperties ?? throw new InvalidOperationException("Not a Struct.");

        public object? Value => IsScalar ? scalarValue : throw new InvalidOperationException("Not a Scalar.");


        public bool IsNull => DataType == WorkflowDataType.Null;
        public bool IsError => DataType == WorkflowDataType.Error;
        public bool IsStruct => DataType == WorkflowDataType.Struct;
        public bool IsArray => DataType == WorkflowDataType.Array;
        public bool IsScalar => DataType != WorkflowDataType.Struct && DataType != WorkflowDataType.Array;

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

        public SomeData()
        {

        }

        public SomeData(WorkflowDataType type, object? value = null)
        {
            if (value is not null)
            {
                this.scalarValue = value;
                DataType = type;
            }
        }

        public SomeData(IEnumerable<SomeData> items)
        {
            DataType = WorkflowDataType.Array;
            this.arrayItems = new List<SomeData>(
                items.Select(i => i.Clone()));
        }

        public SomeData(IDictionary<string, SomeData> structure)
        {
            DataType = WorkflowDataType.Struct;
            this.structProperties = structure;
        }

        public SomeData(IList<SomeData> list)
        {
            DataType = WorkflowDataType.Array;
            this.arrayItems = list;
        }

        public SomeData(IEnumerable<KeyValuePair<string, SomeData>> properties)
        {
            DataType = WorkflowDataType.Struct;
            this.structProperties = new Dictionary<string, SomeData>();
            foreach (var entry in properties)
            {
                this.structProperties.Add(entry.Key, entry.Value);
            }
        }


        public SomeData this[string name]
        {
            get 
            {
                return structProperties?.GetValueOrDefault(name) ?? default;
            }
            set 
            {
                Properties[name] = value;
            }
        }

        public SomeData this[int index]
        {
            get
            {
                return arrayItems?.ElementAtOrDefault(index) ?? default;
            }
            set
            {
                Items[index] = value;
            }
        }

        public static SomeData Undef()
        {
            return new SomeData();
        }

        public static SomeData Integer(Int32? value)
        {
            return new SomeData(WorkflowDataType.Integer, value);
        }

        public static SomeData Integer(Int64? value)
        {
            return new SomeData(WorkflowDataType.Integer, value);
        }

        public static SomeData Boolean(bool? value)
        {
            return new SomeData(WorkflowDataType.Boolean, value);
        }

        public static SomeData String(string? value)
        {
            return new SomeData(WorkflowDataType.String, value);
        }

        public static SomeData Name(string? value)
        {
            return new SomeData(WorkflowDataType.Name, value == null ? null : new WorkflowVariableRef(value));
        }

        public static SomeData Variable(WorkflowVariableRef? value)
        {
            return new SomeData(WorkflowDataType.Name, value);
        }

        public static SomeData Float(Single? value)
        {
            return new SomeData(WorkflowDataType.Single, value);
        }

        public static SomeData Float(Double? value)
        {
            return new SomeData(WorkflowDataType.Single, value);
        }

        public static SomeData Error(WorkflowError? value)
        {
            return new SomeData(WorkflowDataType.Error, value);
        }

        public static SomeData FromPolymorphicObject(object? value)
        {
            if (value == null)
            {
                return SomeData.Undef();
            }
            else if(value is SomeData someData)
            {
                return someData;
            }
            else if (value is IDictionary properties)
            {
                SomeData structure = SomeData.Struct();
                foreach (DictionaryEntry entry in properties)
                {
                    structure.Properties.Add((string)entry.Key, SomeData.FromPolymorphicObject(entry.Value));
                }
                return structure;
            }
            else if (value is IEnumerable items and not string)
            {
                SomeData array = SomeData.Array();
                foreach (object? item in items)
                {
                    array.Items.Add(FromPolymorphicObject(item));
                }
                return array;
            }
            else
            {
                return value switch
                {
                    Int32 v => Integer(v),
                    Int64 v => Integer(v),
                    String v => String(v),
                    Boolean v => Boolean(v),
                    Single v => Float(v),
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
            return new SomeData(WorkflowDataType.Array, items);
        }

        public static SomeData Array(IList<SomeData> items)
        {
            return new SomeData(items);
        }

        public static SomeData Array(IEnumerable<SomeData> items)
        {
            return new SomeData(WorkflowDataType.Array, items);
        }

        public static explicit operator Int32(SomeData value) => value.ToInt32();
        public static explicit operator String(SomeData value) => value.ToString();
        public static explicit operator Boolean(SomeData value) => value.ToBoolean();

        public int ToInt32()
        {
            return Convert.ToInt32(scalarValue);
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

        public bool ToBoolean()
        {
            return Convert.ToBoolean(scalarValue);
        }

        public dynamic? ToDynamic()
        {
            return scalarValue;
        }

        public WorkflowMethodRef ToMethodRef()
        {
            if (scalarValue == null)
            {
                return default;
            }
            else
            {
                return (WorkflowMethodRef)scalarValue;
            }
        }

        public WorkflowOutputRef ToOutputRef()
        {
            return (WorkflowOutputRef)scalarValue!;
        }

        public WorkflowVariableRef ToVariableRef()
        {
            return (WorkflowVariableRef)scalarValue!;
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

        public SomeData Clone()
        {
            if (IsArray)
            {
                return SomeData.Array(arrayItems!.Select(i => i.Clone()));
            }
            else if (IsStruct)
            {
                SomeData result = SomeData.Struct();
                foreach (KeyValuePair<string, SomeData> pair in Properties!)
                    result.Properties.Add(pair.Key, pair.Value);

                return result;
            }
            else
            {
                return FromPolymorphicObject(scalarValue);
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

        public SomeData ResolveTraversal()
        {
            return this;
        }
    }
}
