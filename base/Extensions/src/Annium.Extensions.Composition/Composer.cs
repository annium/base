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

public abstract class Composer<TValue> : ICompositionContainer<TValue> where TValue : class
{
    public IEnumerable<PropertyInfo> Fields => _rules.Keys;

    private readonly IDictionary<PropertyInfo, IRuleContainer<TValue>> _rules =
        new Dictionary<PropertyInfo, IRuleContainer<TValue>>();

    protected IRuleBuilder<TValue, TField> Field<TField>(
        Expression<Func<TValue, TField>> targetAccessor,
        bool allowDefault = false
    )
    {
        var target = TypeHelper.ResolveProperty(targetAccessor);
        var targetSetter = target.GetSetMethod(true) ??
            throw new ArgumentException("Target property has no setter", nameof(targetAccessor));

        var rule = new RuleContainer<TValue, TField>(targetSetter, allowDefault);

        _rules[target] = rule;

        return rule;
    }

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