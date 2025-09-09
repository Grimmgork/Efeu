using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Efeu.Runtime.Data
{
    public enum EfeuValueTag : byte
    {
        Nil = 0,
        True = 1,
        False = 2,
        Integer = 3,
        Object = 4,
    }

    public struct EfeuValue
    {
        public readonly EfeuValueTag Tag;
        private readonly long integer;
        private readonly EfeuObject? obj;

        public EfeuValue(long i) {
            Tag = EfeuValueTag.Integer;
            obj = null;
            integer = i; 
        }
        public EfeuValue(bool b) {
            Tag = b ? EfeuValueTag.True : EfeuValueTag.False;
            obj = null;
            integer = 0;
        }
        public EfeuValue(EfeuObject obj) {
            Tag = EfeuValueTag.Object;
            this.obj = obj; 
            integer = 0;
        }

        public static implicit operator EfeuValue(int i) => new EfeuValue(i);
        public static implicit operator EfeuValue(long i) => new EfeuValue(i);
        public static implicit operator EfeuValue(bool b) => new EfeuValue(b);
        public static implicit operator EfeuValue(double d) => new EfeuValue(new EfeuFloat(d));
        public static implicit operator EfeuValue(decimal d) => new EfeuValue(new EfeuDecimal(d));
        public static implicit operator EfeuValue(DateTime dt) => new EfeuValue(new EfeuTime(dt));
        public static implicit operator EfeuValue(EfeuObject obj) => new EfeuValue(obj);

        public static EfeuValue Parse(object? value)
        {
            if (value is null)
            {
                return EfeuValue.Nil(); 
            }
            return value switch
            {
                UInt16 v => v,
                UInt32 v => v,
                Int16 v => v,
                Int32 v => v,
                Int64 v => v,
                String v => new EfeuString(v),
                Boolean v => v,
                Double v => v,
                Decimal v => new EfeuDecimal(v),
                DateTime v => new EfeuTime(v),
                _ => value.GetType().IsClass ? new EfeuWrapper(value) : throw new InvalidCastException($"Cant parse type {value.GetType()} as {nameof(EfeuValue)}.")
            };
        }

        public static EfeuValue operator ++(EfeuValue value)
        {
            if (value.Tag == EfeuValueTag.Integer)
            {
                return value.ToLong() + 1;
            }
            else if (value.Tag == EfeuValueTag.Object)
            {
                return value.AsObject().Increment();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static EfeuValue operator --(EfeuValue value)
        {
            if (value.Tag == EfeuValueTag.Integer)
            {
                return value.ToLong() - 1;
            }
            else if (value.Tag == EfeuValueTag.Object)
            {
                return value.AsObject().Decrement();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public bool IsNil()
        {
            return Tag == EfeuValueTag.Nil;
        }

        public int ToInt()
        {
            return (Int32)ToLong();
        }

        public IEnumerable<EfeuValue> Each()
        {
            if (Tag == EfeuValueTag.Object)
            {
                if (obj is IEnumerable<EfeuValue> enumerable)
                {
                    return enumerable;
                }
            }

            return Enumerable.Empty<EfeuValue>();
        }

        public void Each(Action<EfeuValue> process)
        {
            foreach (EfeuValue item in Each())
            {
                process(item);
            }
        }

        public int Length()
        {
            if (Tag == EfeuValueTag.Object)
            {
                if (obj is EfeuArray array)
                {
                    return array.Length;
                }
                if (obj is ICollection<EfeuValue> collection)
                {
                    return collection.Count;
                }
                if (obj is IEnumerable<EfeuValue> enumerable)
                {
                    return enumerable.Count();
                }
            }

            return 0;
        }

        public long ToLong()
        {
            if (Tag == EfeuValueTag.Integer)
            {
                return integer;
            }
            else if (Tag == EfeuValueTag.Nil)
            {
                return 0;
            }
            else if (Tag == EfeuValueTag.True)
            {
                return 1;
            }
            else if (Tag == EfeuValueTag.False)
            {
                return 0;
            }
            else if (Tag == EfeuValueTag.Object)
            {
                return obj!.ToLong();
            }

            throw new InvalidCastException("Value is not an integer.");
        }

        public float ToFloat()
        {
            return (float)ToLong();
        }

        public double ToDouble()
        {
            if (Tag == EfeuValueTag.Integer)
            {
                return integer;
            }
            else if (Tag == EfeuValueTag.Object)
            {
                return obj!.ToDouble();
            }
            else if (Tag == EfeuValueTag.Nil)
            {
                return 0;
            }
            else if (Tag == EfeuValueTag.True)
            {
                return 1;
            }
            else if (Tag == EfeuValueTag.False)
            {
                return 0;
            }

            throw new InvalidCastException("Value is not a double.");
        }

        public bool ToBoolean()
        {
            if (Tag == EfeuValueTag.True)
            {
                return true;
            }
            else if (Tag == EfeuValueTag.False)
            {
                return false;
            }
            else if (Tag == EfeuValueTag.Nil)
            {
                return false;
            }
            else if (Tag == EfeuValueTag.Integer)
            {
                return integer == 0;
            }
            else if (Tag == EfeuValueTag.Object)
            {
                return obj!.ToBoolean();
            }

            throw new InvalidCastException("Value is not a bool.");
        }

        public override string ToString()
        {
            if (Tag == EfeuValueTag.Nil)
            {
                return "Nil";
            }
            else if (Tag == EfeuValueTag.True)
            {
                return "True";
            }
            else if (Tag == EfeuValueTag.False)
            {
                return "False";
            }
            else if (Tag == EfeuValueTag.Integer)
            {
                return integer.ToString();
            }
            else if (Tag == EfeuValueTag.Object)
            {
                return obj!.ToString();
            }

            throw new InvalidCastException("value is not a string.");
        }

        public EfeuObject AsObject()
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!;
            }

            throw new InvalidCastException("Value is not an object.");
        }

        public T As<T>() where T : EfeuObject
        {
            EfeuObject obj = AsObject();
            return (T)obj;
        }

        public override int GetHashCode()
        {
            if (Tag == EfeuValueTag.Nil)
            {
                return 0;
            }
            else if (Tag == EfeuValueTag.True)
            {
                return true.GetHashCode();
            }
            else if (Tag == EfeuValueTag.False)
            {
                return false.GetHashCode();
            }
            else if (Tag == EfeuValueTag.Integer)
            {
                return integer.GetHashCode();
            }
            else
            {
                return obj!.GetHashCode();
            }
        }


        public EfeuValue Call(string field)
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Call(field);
            }
            else
            {
                return EfeuValue.Nil();
            }
        }

        public void Call(string field, EfeuValue value)
        {
            if (Tag == EfeuValueTag.Object)
            {
                obj!.Call(field, value);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public EfeuValue Call(int index)
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Call(index);
            }
            else
            {
                return EfeuValue.Nil();
            }
        }

        public void Call(int index, EfeuValue value)
        {
            if (Tag == EfeuValueTag.Object)
            {
                obj!.Call(index, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Push(EfeuValue item)
        {
            if (Tag == EfeuValueTag.Object)
            {
                obj!.Push(item);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public EfeuValue Pop()
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Pop();
            }

            return EfeuValue.Nil();
        }

        public static EfeuValue Wrap<T>(T wrappee) where T : class
        {
            if (wrappee is EfeuObject obj)
            {
                return obj;
            }

            return new EfeuWrapper(wrappee);
        }

        public static EfeuValue Array(params EfeuValue[] items)
        {
            return new EfeuArray(items);
        }

        public static EfeuValue Array(IEnumerable<EfeuValue> items)
        {
            return new EfeuArray(items);
        }

        public static EfeuValue Hash()
        {
            return new EfeuHash();
        }

        public static EfeuValue Nil()
        {
            return default;
        }
    }

    public abstract class EfeuObject
    {
        public string Name => this.GetType().Name;

        public virtual EfeuValue Call(string field)
        {
            return EfeuValue.Nil();
        }

        public virtual void Call(string field, EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual void Call(int index, EfeuValue value)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Call(int index)
        {
            return EfeuValue.Nil();
        }

        public virtual EfeuValue Increment()
        {
            return ToLong() + 1;
        }

        public virtual EfeuValue Decrement()
        {
            return ToLong() - 1;
        }

        public virtual void Push(EfeuValue item)
        {
            throw new NotImplementedException();
        }

        public virtual EfeuValue Pop()
        {
            throw new NotImplementedException();
        }

        public virtual long ToLong()
        {
            throw new NotImplementedException();
        }

        public virtual bool ToBoolean()
        {
            return ToLong() == 0;
        }

        public virtual decimal ToDecimal()
        {
            return ToLong();
        }

        public virtual double ToDouble()
        {
            return ToLong();
        }

        public override string ToString()
        {
            return $"<{Name}>";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class EfeuString : EfeuObject
    {
        private readonly string Text;

        public EfeuString(string value) 
        {
            this.Text = value; 
        }

        public override string ToString()
        {
            return Text;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }

    public class EfeuFloat : EfeuObject
    {
        public readonly double Value;

        public EfeuFloat(double value)
        {
            this.Value = value;
        }

        public override bool ToBoolean()
        {
            return Value == 0;
        }

        public override double ToDouble()
        {
            return Value;
        }

        public override decimal ToDecimal()
        {
            return (decimal)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class EfeuDecimal : EfeuObject
    {
        public readonly decimal Value;

        public EfeuDecimal(decimal value)
        {
            Value = value;
        }

        public override double ToDouble()
        {
            return (double)Value;
        }

        public override bool ToBoolean()
        {
            return Value == 0;
        }

        public override decimal ToDecimal()
        {
            return Value;
        }

        public override long ToLong()
        {
            return (long)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
    public class EfeuWrapper : EfeuObject
    {
        public readonly object Wrappee;

        public EfeuWrapper(object wrappee)
        {
            this.Wrappee = wrappee;
        }

        public override EfeuValue Call(string field)
        {
            Type type = Wrappee.GetType();
            return EfeuValue.Parse(type.GetProperty(field)?.GetValue(Wrappee, null));
        }

        public override EfeuValue Call(int index)
        {
            dynamic obj = Wrappee;
            return EfeuValue.Parse(obj[index]);
        }

        public override bool ToBoolean()
        {
            return Convert.ToBoolean(Wrappee);
        }

        public override decimal ToDecimal()
        {
            return Convert.ToDecimal(Wrappee);
        }

        public override double ToDouble()
        {
            return Convert.ToDouble(Wrappee);
        }

        public override long ToLong()
        {
            return Convert.ToInt64(Wrappee);
        }

        public override string ToString()
        {
            return Wrappee.ToString() ?? "";
        }
    }

    public class EfeuTime : EfeuObject
    {
        public readonly DateTimeOffset Timestamp;

        public EfeuTime(long seconds, int milliseconds = 0)
        {
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(seconds).AddMilliseconds(milliseconds);
        }

        public EfeuTime()
        {
            Timestamp = DateTimeOffset.MinValue;
        }

        public EfeuTime(DateTime dt)
        {
            Timestamp = dt;
        }

        public EfeuTime(DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
        }

        public static EfeuTime Now => new EfeuTime(DateTimeOffset.Now);

        public override long ToLong()
        {
            return Timestamp.ToUnixTimeSeconds();
        }

        public override string ToString()
        {
            return Timestamp.ToLocalTime().ToString();
        }
    }

    public class EfeuHash : EfeuObject
    {
        public readonly IDictionary<string, EfeuValue> Fields = new Dictionary<string, EfeuValue>();

        public EfeuValue this[string field]
        {
            get
            {
                return Fields.GetValueOrDefault(field, default);
            }
            set
            {
                Fields[field] = value;
            }
        }

        public override EfeuValue Call(string field)
        {
            return Fields.GetValueOrDefault(field, EfeuValue.Nil());
        }

        public override void Call(string field, EfeuValue value)
        {
            Fields[field] = value;
        }
    }

    public class EfeuArray : EfeuObject, IEnumerable<EfeuValue>
    {
        public readonly IList<EfeuValue> Items = new List<EfeuValue>();

        public int Length => Items.Count;

        public EfeuValue this[int index]
        {
            get
            {
                return Items.ElementAtOrDefault(index);
            }
            set
            {
                Items[index] = value;
            }
        }

        public EfeuArray(IEnumerable<EfeuValue> items)
        {
            foreach (EfeuValue item in items)
                Items.Add(item);
        }

        public EfeuArray(params EfeuValue[] items)
        {
            foreach (EfeuValue item in items)
                Items.Add(item);
        }

        public EfeuArray()
        {

        }

        public override EfeuValue Call(int index)
        {
            return Items.ElementAtOrDefault(index);
        }

        public override void Call(int index, EfeuValue value)
        {
            Items[index] = value;
        }

        public override void Push(EfeuValue value)
        {
            Items.Add(value);
        }

        public override EfeuValue Pop()
        {
            EfeuValue item = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
            return item;
        }

        public void Prepend(EfeuValue value)
        {
            Items.Prepend(value);
        }

        public IEnumerator<EfeuValue> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
