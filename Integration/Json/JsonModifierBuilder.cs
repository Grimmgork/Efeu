using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Efeu.Integration.Json;

using System.Linq.Expressions;
using System.Text.Json.Serialization.Metadata;

public static class JsonModifierBuilder
{
    public static Builder<T> For<T>() => new();

    public sealed class Builder<T>
    {
        private readonly List<Action<JsonTypeInfo>> _modifiers = [];

        public Builder<T> IgnoreWhen<TProp>(
            Expression<Func<T, TProp>> propertyExpression,
            Func<TProp, bool> predicate)
        {
            if (propertyExpression.Body is not MemberExpression member)
                throw new ArgumentException("Expression must be a property or field.");

            var propertyName = member.Member.Name;

            _modifiers.Add(typeInfo =>
            {
                if (typeInfo.Type != typeof(T))
                    return;

                var property = typeInfo.Properties.FirstOrDefault(p => p.Name == propertyName);
                if (property is null)
                    return;

                property.ShouldSerialize = (obj, value) => !predicate((TProp)value!);
            });

            return this;
        }

        public Builder<T> IgnoreWhenNull<TProp>(
            Expression<Func<T, TProp>> propertyExpression)
        {
            return IgnoreWhen(propertyExpression, (value) => value is null);
        }

        public Builder<T> IgnoreWhenEquals<TMember>(
            Expression<Func<T, TMember>> propertyExpression, TMember equals)
        {
            return IgnoreWhen(propertyExpression, (value) => value?.Equals(equals) ?? equals is null);
        }

        public Builder<T> IgnoreWhenEmpty(
            Expression<Func<T, object?>> propertyExpression)
        {
            return IgnoreWhen(propertyExpression, (value) =>
            {
                if (value is IEnumerable enumerable)
                {
                    return !enumerable.Cast<object>().Any();
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

        public Action<JsonTypeInfo> Build()
        {
            return typeInfo =>
            {
                foreach (var modifier in _modifiers)
                {
                    modifier(typeInfo);
                }
            };
        }
    }
}