using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Composition.Internal;
using Annium.Localization.Abstractions;
using Annium.Reflection;

namespace Annium.Extensions.Composition;

/// <summary>
/// Abstract base class for implementing value composition with field rules
/// </summary>
/// <typeparam name="TValue">The type of value to compose</typeparam>
public abstract class Composer<TValue> : ICompositionContainer<TValue>
    where TValue : class
{
    /// <summary>
    /// Gets the collection of fields that have rules configured
    /// </summary>
    public IEnumerable<PropertyInfo> Fields => _rules.Keys;

    /// <summary>
    /// Dictionary of field rules keyed by property info
    /// </summary>
    private readonly IDictionary<PropertyInfo, IRuleContainer<TValue>> _rules =
        new Dictionary<PropertyInfo, IRuleContainer<TValue>>();

    /// <summary>
    /// Configures a field rule for the specified property
    /// </summary>
    /// <typeparam name="TField">The type of the field</typeparam>
    /// <param name="targetAccessor">Expression to access the target property</param>
    /// <param name="allowDefault">Whether to allow default values</param>
    /// <returns>A rule builder for the field</returns>
    protected IRuleBuilder<TValue, TField> Field<TField>(
        Expression<Func<TValue, TField>> targetAccessor,
        bool allowDefault = false
    )
    {
        var target = TypeHelper.ResolveProperty(targetAccessor);
        var targetSetter =
            target.GetSetMethod(true)
            ?? throw new ArgumentException("Target property has no setter", nameof(targetAccessor));

        var rule = new RuleContainer<TValue, TField>(targetSetter, allowDefault);

        _rules[target] = rule;

        return rule;
    }

    /// <summary>
    /// Composes the specified value by applying all configured rules
    /// </summary>
    /// <param name="value">The value to compose</param>
    /// <param name="label">The label for the composition context</param>
    /// <param name="localizer">The localizer for error messages</param>
    /// <returns>The result of the composition operation</returns>
    public async Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label, ILocalizer localizer)
    {
        var result = Result.New();
        var hasLabel = !string.IsNullOrWhiteSpace(label);

        foreach (var (property, rule) in _rules)
        {
            var propertyLabel = hasLabel ? $"{label}.{property.Name}" : property.Name;
            var ruleResult = Result.New();
            var context = new CompositionContext<TValue>(value, propertyLabel, property.Name, ruleResult, localizer);

            await rule.ComposeAsync(context, value);

            result.Join(ruleResult);
        }

        return Result.Status(result.IsOk ? OperationStatus.Ok : OperationStatus.NotFound).Join(result);
    }
}
