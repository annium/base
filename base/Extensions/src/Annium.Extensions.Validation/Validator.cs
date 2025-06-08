using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Validation.Internal;
using Annium.Localization.Abstractions;
using Annium.Reflection;

namespace Annium.Extensions.Validation;

/// <summary>
/// Base class for implementing typed validators that define validation rules for objects
/// </summary>
/// <typeparam name="TValue">The type of object to validate</typeparam>
public abstract class Validator<TValue> : IValidationContainer<TValue>
{
    /// <summary>
    /// Collection of validation rules organized by property
    /// </summary>
    private readonly IDictionary<PropertyInfo, IRuleContainer<TValue>> _rules =
        new Dictionary<PropertyInfo, IRuleContainer<TValue>>();

    /// <summary>
    /// Creates a rule builder for the specified property field
    /// </summary>
    /// <typeparam name="TField">The type of the field to validate</typeparam>
    /// <param name="accessor">Expression that identifies the property to validate</param>
    /// <returns>A rule builder for configuring validation rules for the field</returns>
    protected IRuleBuilder<TValue, TField> Field<TField>(Expression<Func<TValue, TField>> accessor)
    {
        var property = TypeHelper.ResolveProperty(accessor);
        var rule = new RuleContainer<TValue, TField>(accessor.Compile());

        _rules[property] = rule;

        return rule;
    }

    /// <summary>
    /// Validates the specified value using all configured rules
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="label">The label prefix for error messages</param>
    /// <param name="stage">The validation stage (used for conditional validation)</param>
    /// <param name="localizer">The localizer for translating error messages</param>
    /// <returns>A tuple containing the validation result and whether any rules ran in the specified stage</returns>
    public async Task<ValueTuple<IResult, bool>> ValidateAsync(
        TValue value,
        string label,
        int stage,
        ILocalizer localizer
    )
    {
        if (_rules.Count == 0)
            return (Result.New(), false);

        var result = Result.New();
        var ranStage = false;
        var hasLabel = !string.IsNullOrWhiteSpace(label);

        foreach (var (property, rule) in _rules)
        {
            var propertyLabel = hasLabel ? $"{label}.{property.Name}" : property.Name;
            var ruleResult = Result.New();
            var context = new ValidationContext<TValue>(value, propertyLabel, property.Name, ruleResult, localizer);

            ranStage = await rule.ValidateAsync(context, value, stage) || ranStage;

            result.Join(ruleResult);
        }

        return (result, ranStage);
    }
}
