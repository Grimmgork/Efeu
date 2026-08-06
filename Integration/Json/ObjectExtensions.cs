using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Efeu.Runtime;

namespace Efeu.Integration.Json;

public static class ObjectExtensions
{
    public static Action<JsonTypeInfo> JsonSkipWhenEmpty<T>(
        Expression<Func<T, object>> propertyExpression)
    {
        return ObjectExtensions.JsonSkipWhen<T, object>(propertyExpression, (value) =>
        {
            if (value is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Any();
            }
            else if (value is string str)
            {
                return string.IsNullOrEmpty(str);
            }
            else
            {
                return true;
            }
        });
    }
    
    public static Action<JsonTypeInfo> JsonSkipWhen<T>(
        Expression<Func<T, object>> propertyExpression, object equals)
    {
        return ObjectExtensions.JsonSkipWhen<T, object>(propertyExpression, (value) => value.Equals(equals));
    }
    
    
    public static Action<JsonTypeInfo> JsonSkipWhen<T, TProp>(
        Expression<Func<T, TProp>> propertyExpression,
        Func<TProp, bool> predicate)
    {
        if (propertyExpression.Body is not MemberExpression member)
            throw new ArgumentException("Expression must be a property or field.");

        var propertyName = member.Member.Name;

        return typeInfo =>
        {
            if (typeInfo.Type != typeof(T))
                return;

            var jsonProperty = typeInfo.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (jsonProperty == null)
                return;

            jsonProperty.ShouldSerialize = (obj, value) =>
            {
                return predicate((TProp)value!);
            };
        };
    }
}