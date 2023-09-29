using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Validation;

public static class ComplexRuleExtensions
{
    public static IRuleBuilder<TValue, TField> Unique<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, Task<bool>> getEntityPresenceAsync,
        string message = ""
    ) => rule.Add(async (context, value) =>
    {
        var exists = await getEntityPresenceAsync(context.Root, value);
        if (exists)
            context.Error(string.IsNullOrEmpty(message) ? "{0} with {1} {2} already exists" : message, typeof(TValue).Name, context.Field, value!);
    });

    public static IRuleBuilder<TValue, TField> Unique<TValue, TField>(
        this IRuleBuilder<TValue, TField> rule,
        Func<TValue, TField, bool> getEntityPresence,
        string message = ""
    ) => rule.Add((context, value) =>
    {
        var exists = getEntityPresence(context.Root, value);
        if (exists)
            context.Error(string.IsNullOrEmpty(message) ? "{0} with {1} {2} already exists" : message, typeof(TValue).Name, context.Field, value!);
    });
}