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

public abstract class Validator<TValue> : IValidationContainer<TValue>
{
    private readonly IDictionary<PropertyInfo, IRuleContainer<TValue>> _rules =
        new Dictionary<PropertyInfo, IRuleContainer<TValue>>();

    protected IRuleBuilder<TValue, TField> Field<TField>(
        Expression<Func<TValue, TField>> accessor
    )
    {
        var property = TypeHelper.ResolveProperty(accessor);
        var rule = new RuleContainer<TValue, TField>(accessor.Compile());

        _rules[property] = rule;

        return rule;
    }

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