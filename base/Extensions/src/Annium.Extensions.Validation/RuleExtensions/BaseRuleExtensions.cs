using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
namespace Annium.Extensions.Validation.RuleExtensions;

/// <summary>
/// Extension methods providing common validation rules for basic data types and scenarios.
/// These rules can be chained together using the fluent interface to build complex validation logic.
/// </summary>
public static class BaseRuleExtensions
{
    /// <summary>
    /// Validates that a string field is not null, empty, or whitespace
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> Required<TValue>(
        this IRuleBuilder<TValue, string> rule,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (string.IsNullOrWhiteSpace(value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is required" : message);
            }
        );

    /// <summary>
    /// Validates that a nullable value type field has a value and is not the default value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The value type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField?> Required<TValue, TField>(
        this IRuleBuilder<TValue, TField?> rule,
        string message = ""
    )
        where TField : struct =>
        rule.Add(
            (context, value) =>
            {
                if (value.HasValue && AreEqual(value, default(TField)))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is required" : message);
            }
        );

    /// <summary>
    /// Validates that a field is not the default value for its type
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Required<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (AreEqual(value, default!))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is required" : message);
            }
        );

    /// <summary>
    /// Validates that a field value equals the specified target value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="target">The target value to compare against</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Equal<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField target,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (!AreEqual(value, target))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is not equal to given" : message);
            }
        );

    /// <summary>
    /// Validates that a field value is contained within the specified collection of valid values
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="targets">Collection of valid values</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> In<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        IEnumerable<TField> targets,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (!targets.Any(target => AreEqual(value, target)))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is not in given" : message);
            }
        );

    /// <summary>
    /// Validates that a field value equals a target value computed from the root validation object
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="target">Function to compute the target value from the root object</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Equal<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField> target,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (!AreEqual(value, target(context.Root)))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is not equal to given" : message);
            }
        );

    /// <summary>
    /// Validates that a field value does not equal the specified target value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="target">The target value that should not be equal</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> NotEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField target,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (AreEqual(value, target))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is equal to given" : message);
            }
        );

    /// <summary>
    /// Validates that a field value is not contained within the specified collection of invalid values
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="targets">Collection of invalid values</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> NotIn<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        IEnumerable<TField> targets,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (targets.Any(target => AreEqual(value, target)))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is in given" : message);
            }
        );

    /// <summary>
    /// Validates that a field value does not equal a target value computed from the root validation object
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="target">Function to compute the target value from the root object</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> NotEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField> target,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (AreEqual(value, target(context.Root)))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is equal to given" : message);
            }
        );

    /// <summary>
    /// Validates that a string field length is within the specified range (inclusive)
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="minLength">Minimum allowed length (inclusive)</param>
    /// <param name="maxLength">Maximum allowed length (inclusive)</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> Length<TValue>(
        this IRuleBuilder<TValue, string> rule,
        int minLength,
        int maxLength,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (value?.Length < minLength)
                    context.Error(
                        string.IsNullOrEmpty(message) ? "Value length is less, than {0}" : message,
                        minLength
                    );

                if (value?.Length > maxLength)
                    context.Error(
                        string.IsNullOrEmpty(message) ? "Value length is greater, than {0}" : message,
                        maxLength
                    );
            }
        );

    /// <summary>
    /// Validates that a string field has at least the specified minimum length
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="minLength">Minimum required length</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> MinLength<TValue>(
        this IRuleBuilder<TValue, string> rule,
        int minLength,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (value?.Length < minLength)
                    context.Error(
                        string.IsNullOrEmpty(message) ? "Value length is less, than {0}" : message,
                        minLength
                    );
            }
        );

    /// <summary>
    /// Validates that a string field does not exceed the specified maximum length
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="maxLength">Maximum allowed length</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> MaxLength<TValue>(
        this IRuleBuilder<TValue, string> rule,
        int maxLength,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (value?.Length > maxLength)
                    context.Error(
                        string.IsNullOrEmpty(message) ? "Value length is greater, than {0}" : message,
                        maxLength
                    );
            }
        );

    /// <summary>
    /// Validates that a comparable field value is between the specified minimum and maximum values (inclusive)
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated, must implement IComparable</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="min">Minimum allowed value (inclusive)</param>
    /// <param name="max">Maximum allowed value (inclusive)</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Between<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField min,
        TField max,
        string message = ""
    )
        where TField : IComparable<TField> =>
        rule.Add(
            (context, value) =>
            {
                if (value?.CompareTo(min) == -1)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is less, than given minimum" : message);

                if (value?.CompareTo(max) == 1)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is greater, than given maximum" : message);
            }
        );

    /// <summary>
    /// Validates that a comparable field value is less than the specified maximum value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated, must implement IComparable</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="max">Maximum value (exclusive)</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> LessThan<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField max,
        string message = ""
    )
        where TField : IComparable<TField> =>
        rule.Add(
            (context, value) =>
            {
                if (value?.CompareTo(max) >= 0)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is greater, than given maximum" : message);
            }
        );

    /// <summary>
    /// Validates that a comparable field value is less than or equal to the specified maximum value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated, must implement IComparable</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="max">Maximum value (inclusive)</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> LessThanOrEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField max,
        string message = ""
    )
        where TField : IComparable<TField> =>
        rule.Add(
            (context, value) =>
            {
                if (value?.CompareTo(max) > 0)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is greater, than given maximum" : message);
            }
        );

    /// <summary>
    /// Validates that a comparable field value is greater than the specified minimum value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated, must implement IComparable</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="min">Minimum value (exclusive)</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> GreaterThan<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField min,
        string message = ""
    )
        where TField : IComparable<TField> =>
        rule.Add(
            (context, value) =>
            {
                if (value?.CompareTo(min) <= 0)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is less, than given minimum" : message);
            }
        );

    /// <summary>
    /// Validates that a comparable field value is greater than or equal to the specified minimum value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated, must implement IComparable</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="min">Minimum value (inclusive)</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> GreaterThanOrEqual<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField min,
        string message = ""
    )
        where TField : IComparable<TField> =>
        rule.Add(
            (context, value) =>
            {
                if (value?.CompareTo(min) < 0)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is less, than given minimum" : message);
            }
        );

    /// <summary>
    /// Validates that a string field matches the specified regular expression pattern
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="regex">Compiled regular expression to match against</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> Matches<TValue>(
        this IRuleBuilder<TValue, string> rule,
        Regex regex,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (value is not null && !regex.IsMatch(value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match specified regex" : message);
            }
        );

    /// <summary>
    /// Validates that a string field matches the specified regular expression pattern
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="regex">Regular expression pattern string to match against</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> Matches<TValue>(
        this IRuleBuilder<TValue, string> rule,
        string regex,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                var re = new Regex(
                    regex,
                    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture
                );
                if (value is not null && !re.IsMatch(value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match specified regex" : message);
            }
        );

    /// <summary>
    /// Validates that a field value satisfies the specified predicate condition
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="predicate">Function that determines if the field value is valid</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TField, bool> predicate,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (!predicate(value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
            }
        );

    /// <summary>
    /// Validates that a field value satisfies the specified predicate condition using both the root object and field value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="predicate">Function that determines if the field value is valid based on root object and field value</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, bool> predicate,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                if (!predicate(context.Root, value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
            }
        );

    /// <summary>
    /// Validates that a field value satisfies the specified asynchronous predicate condition
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="predicate">Asynchronous function that determines if the field value is valid</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TField, Task<bool>> predicate,
        string message = ""
    ) =>
        rule.Add(
            async (context, value) =>
            {
                if (!await predicate(value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
            }
        );

    /// <summary>
    /// Validates that a field value satisfies the specified asynchronous predicate condition using both the root object and field value
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="predicate">Asynchronous function that determines if the field value is valid based on root object and field value</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Must<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, Task<bool>> predicate,
        string message = ""
    ) =>
        rule.Add(
            async (context, value) =>
            {
                if (!await predicate(context.Root, value))
                    context.Error(string.IsNullOrEmpty(message) ? "Value doesn't match condition" : message);
            }
        );

    /// <summary>
    /// Validates that a string field contains a basic email format (contains '@' with characters before and after)
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, string> Email<TValue>(
        this IRuleBuilder<TValue, string> rule,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                var index = value?.IndexOf('@');
                if (!index.HasValue || index.Value < 1 || index >= value!.Length - 1)
                    context.Error(string.IsNullOrEmpty(message) ? "Value is not an email" : message);
            }
        );

    /// <summary>
    /// Validates that an enum field value is within the valid range of the enum type
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The enum type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Enum<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        string message = ""
    )
        where TField : struct, Enum
    {
        return rule.Add(
            (context, value) =>
            {
                if (!value.TryParseEnum<TField>(out _))
                    context.Error(string.IsNullOrEmpty(message) ? "Value is not in expected range" : message);
            }
        );
    }

    /// <summary>
    /// Validates that a field value equals the specified target value (alias for Equal method)
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="target">The target value to compare against</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Equals<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        TField target,
        string message = ""
    ) => rule.Equal(target, message);

    /// <summary>
    /// Compares two values for equality using the default equality comparer
    /// </summary>
    /// <typeparam name="TField">The type of values to compare</typeparam>
    /// <param name="x">The first value to compare</param>
    /// <param name="y">The second value to compare</param>
    /// <returns>True if the values are equal, false otherwise</returns>
    private static bool AreEqual<TField>(TField x, TField y) => EqualityComparer<TField>.Default.Equals(x, y);
}
