using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Efeu.Router.Data
{
    public enum EfeuValueTag : byte
    {
        Nil = 0,
        True = 1,
        False = 2,
        Integer = 3,
        Object = 4,
    }

    public readonly struct EfeuValue
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
        public static implicit operator EfeuValue(string s) => new EfeuValue(new EfeuString(s));
        public static implicit operator EfeuValue(char c) => new EfeuValue(new EfeuString(c.ToString()));
        public static implicit operator EfeuValue(DateTime dt) => new EfeuValue(new EfeuTime(dt));
        public static implicit operator EfeuValue(EfeuObject obj) => new EfeuValue(obj);

        public static EfeuValue Parse(object? value)
        {
            if (value is null)
            {
                return EfeuValue.Nil();
            }
            if (value is IDictionary dictionary)
            {
                return new EfeuHash(dictionary.Cast<DictionaryEntry>().Select(p =>
                            new KeyValuePair<string, EfeuValue>((string)p.Key, EfeuValue.Parse(p.Value))));
            }
            if (value is IEnumerable enumerable and not string)
            {
                EfeuArray array = new EfeuArray(enumerable.Cast<object?>().Select(EfeuValue.Parse));
                return array;
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
                Decimal v => new EfeuDecimal(v).ToValue(),
                DateTime v => new EfeuTime(v),
                _ => value.GetType().IsClass ? new EfeuWrapper(value) : throw new InvalidCastException($"Cant parse type {value.GetType()} as {nameof(EfeuValue)}.")
            };
        }

        public bool IsNil()
        {
            return Tag == EfeuValueTag.Nil;
        }

        public bool IsObject()
        {
            return Tag == EfeuValueTag.Object;
        }

        public int ToInt()
        {
            return (Int32)ToLong();
        }

        public EfeuValue First()
        {
            if (Tag == EfeuValueTag.Object)
            {
                obj!.First();
            }

            throw new InvalidOperationException();
        }

        public EfeuValue Last()
        {
            if (Tag == EfeuValueTag.Object)
            {
                obj!.Last();
            }

            throw new InvalidOperationException();
        }

        public IEnumerable<EfeuValue> Each()
        {
            if (Tag == EfeuValueTag.Object)
            {
                if (obj is IEnumerable<EfeuValue> enumerable)
                {
                    return enumerable;
                }
                else
                {
                    return obj?.Each() ?? [];
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

        public void Each(Action<EfeuValue, int> process)
        {
            int index = 0;
            foreach (EfeuValue item in Each())
            {
                process(item, index);
            }
        }

        public IEnumerable<KeyValuePair<string, EfeuValue>> Fields()
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Fields();
            }

            return [];
        }

        public int Length()
        {
            if (Tag == EfeuValueTag.Object)
            {
                if (obj is EfeuArray array)
                {
                    return array.Length();
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

        public bool ToBool()
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
                return integer > 0;
            }
            else if (Tag == EfeuValueTag.Object)
            {
                return obj!.ToBoolean();
            }

            throw new InvalidCastException("Value is not a bool.");
        }

        public decimal ToDecimal()
        {
            if (Tag == EfeuValueTag.Integer)
            {
                return integer;
            }
            else if (Tag == EfeuValueTag.Object)
            {
                return obj!.ToDecimal();
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

            throw new InvalidCastException("Value is not a decimal.");
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

        public EfeuHash AsHash()
        {
            return As<EfeuHash>();
        }

        public EfeuArray AsArray()
        {
            return As<EfeuArray>();
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

        public EfeuValue Call(string field, EfeuValue value)
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Call(field, value);
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

        public EfeuValue Call(int index, EfeuValue value)
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Call(index, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public EfeuValue Push(params EfeuValue[] items)
        {
            if (Tag == EfeuValueTag.Object)
            {
                return obj!.Push(items);
            }
            else
            {
                return new EfeuArray([this, ..items]);
            }
        }

        public static EfeuValue Nil()
        {
            return default;
        }

        public bool Equals(EfeuValue value)
        {
            if (this.Tag == EfeuValueTag.Nil)
            {
                return value.Tag == EfeuValueTag.Nil;
            }

            if (this.Tag == EfeuValueTag.True)
            {
                return value.Tag != EfeuValueTag.True;
            }

            if (this.Tag == EfeuValueTag.False)
            {
                return value.Tag != EfeuValueTag.False;
            }

            if (this.Tag == EfeuValueTag.Integer)
            {
                return this.integer == value.ToLong();
            }

            if (this.Tag == EfeuValueTag.Object)
            {
                return this.obj!.Equals(value.AsObject());
            }

            return base.Equals(obj);
        }

        public static EfeuValue operator +(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().Add(right);
            }
            else
            {
                return left.ToDecimal() + right.ToDecimal();
            }
        }

        public static EfeuValue operator -(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().Subtract(right);
            }
            else
            {
                return left.ToDecimal() - right.ToDecimal();
            }
        }

        public static EfeuValue operator *(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().Multiply(right);
            }
            else
            {
                return left.ToDecimal() * right.ToDecimal();
            }
        }

        public static EfeuValue operator /(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().Divide(right);
            }
            else
            {
                return left.ToDecimal() / right.ToDecimal();
            }
        }

        public static EfeuValue operator %(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().Modulo(right);
            }
            else
            {
                return left.ToDecimal() % right.ToDecimal();
            }
        }

        public static EfeuValue operator <(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().LessThan(right);
            }
            else
            {
                return left.ToDecimal() < right.ToDecimal();
            }
        }

        public static EfeuValue operator >(EfeuValue left, EfeuValue right)
        {
            if (left.Tag == EfeuValueTag.Object)
            {
                return left.AsObject().GreaterThan(right);
            }
            else
            {
                return left.ToDecimal() > right.ToDecimal();
            }
        }

        public static EfeuValue operator ==(EfeuValue left, EfeuValue right) => left.Equals(right);

        public static EfeuValue operator !=(EfeuValue left, EfeuValue right) => !left.Equals(right);

        public static bool operator true(EfeuValue x) => x.ToBool();

        public static bool operator false(EfeuValue x) => !x.ToBool();

        public static bool operator !(EfeuValue x) => !x;

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
    }
}
