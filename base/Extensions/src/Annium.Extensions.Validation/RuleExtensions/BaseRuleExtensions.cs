using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Validation;

public static class BaseRuleExtensions
{
    public static IRuleBuilder<TValue, string> Required<TValue>(
        this IRuleBuilder<TValue, string> rule,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (string.IsNullOrWhiteSpace(value))
            context.Error(string.IsNullOrEmpty(message) ? "Value is required" : message);
    });

    public static IRuleBuilder<TValue, TField?> Required<TValue, TField>(
        this IRuleBuilder<TValue, TField?> rule,
        string message = ""
    ) where TField : struct => rule.Add((context, value) =>
    {
        if (value.HasValue && Equals(value, default(TField)))
            context.Error(string.IsNullOrEmpty(message) ? "Value is required" : message);
    });

    public static IRuleBuilder<TValue, TField> Required<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (Equals(value, default!))
            context.Error(string.IsNullOrEmpty(message) ? "Value is required" : message);
    });

    public static IRuleBuilder<TValue, TField> Equal<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField target,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (!Equals(value, target))
            context.Error(string.IsNullOrEmpty(message) ? "Value is not equal to given" : message);
    });

    public static IRuleBuilder<TValue, TField> In<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        IEnumerable<TField> targets,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (!targets.Any(target => Equals(value, target)))
            context.Error(string.IsNullOrEmpty(message) ? "Value is not in given" : message);
    });

    public static IRuleBuilder<TValue, TField> Equal<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField> target,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (!Equals(value, target(context.Root)))
            context.Error(string.IsNullOrEmpty(message) ? "Value is not equal to given" : message);
    });

    public static IRuleBuilder<TValue, TField> NotEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField target,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (Equals(value, target))
            context.Error(string.IsNullOrEmpty(message) ? "Value is equal to given" : message);
    });

    public static IRuleBuilder<TValue, TField> NotIn<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        IEnumerable<TField> targets,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (targets.Any(target => Equals(value, target)))
            context.Error(string.IsNullOrEmpty(message) ? "Value is in given" : message);
    });

    public static IRuleBuilder<TValue, TField> NotEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField> target,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (Equals(value, target(context.Root)))
            context.Error(string.IsNullOrEmpty(message) ? "Value is equal to given" : message);
    });

    public static IRuleBuilder<TValue, string> Length<TValue>(
        this IRuleBuilder<TValue, string> rule,
        int minLength,
        int maxLength,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (value?.Length < minLength)
            context.Error(string.IsNullOrEmpty(message) ? "Value length is less, than {0}" : message, minLength);

        if (value?.Length > maxLength)
            context.Error(string.IsNullOrEmpty(message) ? "Value length is greater, than {0}" : message, maxLength);
    });

    public static IRuleBuilder<TValue, string> MinLength<TValue>(
        this IRuleBuilder<TValue, string> rule,
        int minLength,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (value?.Length < minLength)
            context.Error(string.IsNullOrEmpty(message) ? "Value length is less, than {0}" : message, minLength);
    });

    public static IRuleBuilder<TValue, string> MaxLength<TValue>(
        this IRuleBuilder<TValue, string> rule,
        int maxLength,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (value?.Length > maxLength)
            context.Error(string.IsNullOrEmpty(message) ? "Value length is greater, than {0}" : message, maxLength);
    });

    public static IRuleBuilder<TValue, TField> Between<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField min,
        TField max,
        string message = ""
    ) where TField : IComparable<TField> => rule.Add((context, value) =>
    {
        if (value?.CompareTo(min) == -1)
            context.Error(string.IsNullOrEmpty(message) ? "Value is less, than given minimum" : message);

        if (value?.CompareTo(max) == 1)
            context.Error(string.IsNullOrEmpty(message) ? "Value is greater, than given maximum" : message);
    });

    public static IRuleBuilder<TValue, TField> LessThan<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField max,
        string message = ""
    ) where TField : IComparable<TField> => rule.Add((context, value) =>
    {
        if (value?.CompareTo(max) >= 0)
            context.Error(string.IsNullOrEmpty(message) ? "Value is greater, than given maximum" : message);
    });

    public static IRuleBuilder<TValue, TField> LessThanOrEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField max,
        string message = ""
    ) where TField : IComparable<TField> => rule.Add((context, value) =>
    {
        if (value?.CompareTo(max) > 0)
            context.Error(string.IsNullOrEmpty(message) ? "Value is greater, than given maximum" : message);
    });

    public static IRuleBuilder<TValue, TField> GreaterThan<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField min,
        string message = ""
    ) where TField : IComparable<TField> => rule.Add((context, value) =>
    {
        if (value?.CompareTo(min) <= 0)
            context.Error(string.IsNullOrEmpty(message) ? "Value is less, than given minimum" : message);
    });

    public static IRuleBuilder<TValue, TField> GreaterThanOrEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField min,
        string message = ""
    ) where TField : IComparable<TField> => rule.Add((context, value) =>
    {
        if (value?.CompareTo(min) < 0)
            context.Error(string.IsNullOrEmpty(message) ? "Value is less, than given minimum" : message);
    });

    public static IRuleBuilder<TValue, string> Matches<TValue>(
        this IRuleBuilder<TValue, string> rule,
        Regex regex,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (value is not null && !regex.IsMatch(value))
            context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match specified regex" : message);
    });

    public static IRuleBuilder<TValue, string> Matches<TValue>(
        this IRuleBuilder<TValue, string> rule,
        string regex,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        var re = new Regex(regex,
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        if (value is not null && !re.IsMatch(value))
            context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match specified regex" : message);
    });

    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TField, bool> predicate,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (!predicate(value))
            context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
    });

    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, bool> predicate,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        if (!predicate(context.Root, value))
            context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
    });

    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TField, Task<bool>> predicate,
        string message = ""
    ) => rule.Add(async (context, value) =>
    {
        if (!await predicate(value))
            context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
    });

    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, Task<bool>> predicate,
        string message = ""
    ) => rule.Add(async (context, value) =>
    {
        if (!await predicate(context.Root, value))
            context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
    });

    public static IRuleBuilder<TValue, string> Email<TValue>(
        this IRuleBuilder<TValue, string> rule,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        var index = value?.IndexOf('@');
        if (!index.HasValue || index.Value < 1 || index >= value!.Length - 1)
            context.Error(string.IsNullOrEmpty(message) ? "Value is not an email" : message);
    });

    public static IRuleBuilder<TValue, TField> Enum<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        string message = ""
    )
        where TField : struct, Enum
    {
        return rule.Add((context, value) =>
        {
            if (!value.TryParseEnum<TField>(out _))
                context.Error(string.IsNullOrEmpty(message) ? "Value is not in expected range" : message);
        });
    }

    private static bool Equals<TField>(TField x, TField y) =>
        EqualityComparer<TField>.Default.Equals(x, y);
}