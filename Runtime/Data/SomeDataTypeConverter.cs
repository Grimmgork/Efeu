using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;

namespace Efeu.Runtime.Data
{
    internal class SomeDataTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return true;
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            return value switch
            {
                string v => SomeData.String(v),
                Int32 v => SomeData.Integer(v),
                Int64 v => SomeData.Long(v),
                Boolean v => SomeData.Boolean(v),
                _ => base.ConvertFrom(context, culture, value)
            };
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            return true;
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        { 
            SomeData data = (SomeData)value!;
            if (data.IsNull)
            {
                return null;
            }
            if (destinationType == typeof(Dictionary<string, object?>))
            {
                if (!data.IsStruct)
                    throw new Exception();

                return data.ToPolymorphicObject();
            }
            if (destinationType == typeof(List<object?>))
            {
                if (!data.IsArray)
                    throw new Exception();

                return data.ToPolymorphicObject();
            }
            if (destinationType == typeof(object?[]))
            {
                if (!data.IsArray)
                    throw new Exception();

                return ((List<object?>)data.ToPolymorphicObject()!).ToArray();
            }
            if (destinationType == typeof(String))
            {
                return data.ToString();
            }
            if (destinationType == typeof(Boolean))
            {
                return data.ToBoolean();
            }
            if (destinationType == typeof(Int32))
            {
                return data.ToInt32();
            }
            if (destinationType == typeof(Int32))
            {
                return data.ToInt32();
            }
            if (destinationType == typeof(Int32))
            {
                return data.ToInt32();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}