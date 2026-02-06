using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;

namespace Efeu.Runtime.Value
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
        public static implicit operator EfeuValue(DateTimeOffset dt) => new EfeuValue(new EfeuTime(dt));
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
                Decimal v => new EfeuDecimal(v),
                DateTime v => new EfeuTime(v),
                _ => value.GetType().IsClass ? new EfeuWrapper(value) : throw new InvalidCastException($"Cant parse type {value.GetType()} as {nameof(EfeuValue)}.")
            };
        }

        public bool IsNil()
        {
            return Tag == EfeuValueTag.Nil;
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

        public void Each(Action<EfeuValue, int> process)
        {
            int index = 0;
            foreach (EfeuValue item in Each())
            {
                process(item, index);
            }
        }

        public long AsLong()
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
                if (obj is EfeuFloat efeuFloat)
                {
                    return (long)efeuFloat.Value;
                }
                if (obj is EfeuTime efeuTime)
                {
                    return efeuTime.Value.ToUnixTimeSeconds();
                }
            }

            throw new InvalidCastException("Value is not an integer.");
        }

        public double AsDouble()
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
                if (obj is EfeuFloat efeuFloat)
                {
                    return efeuFloat.Value;
                }
            }

            throw new InvalidCastException("Value is not a double.");
        }

        public bool AsBool()
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
                if (obj is EfeuString efeuString)
                {
                    return string.IsNullOrEmpty(efeuString.ToString());
                }

                if (obj is EfeuDecimal efeuDecimal)
                {
                    return efeuDecimal.Value != 0;
                }

                if (obj is EfeuFloat efeuFloat)
                {
                    return efeuFloat.Value != 0;
                }

                if (obj is EfeuArray efeuArray)
                {
                    return efeuArray.Count != 0;
                }

                if (obj is EfeuRange efeuRange)
                {
                    return efeuRange.Any();
                }
            }

            throw new InvalidCastException("Value is not a bool.");
        }

        public decimal AsDecimal()
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
                if (obj is EfeuDecimal efeuDecimal)
                {
                    return efeuDecimal.Value;
                }

                if (obj is EfeuFloat efeuFloat)
                {
                    return (decimal)efeuFloat.Value;
                }
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

        public EfeuHash AsHash()
        {
            EfeuObject obj = AsObject();
            return (EfeuHash)obj;
        }

        public EfeuArray AsArray()
        {
            EfeuObject obj = AsObject();
            return (EfeuArray)obj;
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
                return this.integer == value.AsLong();
            }

            if (this.Tag == EfeuValueTag.Object)
            {
                return this.obj!.Equals(value.AsObject());
            }

            return base.Equals(obj);
        }

        public static EfeuValue operator ==(EfeuValue left, EfeuValue right) => left.Equals(right);

        public static EfeuValue operator !=(EfeuValue left, EfeuValue right) => !left.Equals(right);

        public static bool operator true(EfeuValue x) => x.AsBool();

        public static bool operator false(EfeuValue x) => !x.AsBool();

        public static bool operator !(EfeuValue x) => !x;
    }
}
