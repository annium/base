using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation.RuleExtensions;

/// <summary>
/// Extension methods providing complex validation rules that require more sophisticated logic or external dependencies
/// </summary>
public static class ComplexRuleExtensions
{
    /// <summary>
    /// Validates that a field value is unique by checking its presence asynchronously using the provided predicate
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="getEntityPresenceAsync">Asynchronous function that checks if an entity with the given field value already exists</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Unique<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, Task<bool>> getEntityPresenceAsync,
        string message = ""
    ) =>
        rule.Add(
            async (context, value) =>
            {
                var exists = await getEntityPresenceAsync(context.Root, value);
                if (exists)
                    context.Error(
                        string.IsNullOrEmpty(message) ? "{0} with {1} {2} already exists" : message,
                        typeof(TValue).Name,
                        context.Field,
                        value!
                    );
            }
        );

    /// <summary>
    /// Validates that a field value is unique by checking its presence synchronously using the provided predicate
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated</typeparam>
    /// <typeparam name="TField">The type of the field being validated</typeparam>
    /// <param name="rule">The rule builder to extend</param>
    /// <param name="getEntityPresence">Function that checks if an entity with the given field value already exists</param>
    /// <param name="message">Custom error message (uses default if empty)</param>
    /// <returns>The rule builder for method chaining</returns>
    public static IRuleBuilder<TValue, TField> Unique<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, bool> getEntityPresence,
        string message = ""
    ) =>
        rule.Add(
            (context, value) =>
            {
                var exists = getEntityPresence(context.Root, value);
                if (exists)
                    context.Error(
                        string.IsNullOrEmpty(message) ? "{0} with {1} {2} already exists" : message,
                        typeof(TValue).Name,
                        context.Field,
                        value!
                    );
            }
        );
}
