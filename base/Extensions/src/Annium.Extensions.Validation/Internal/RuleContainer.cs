using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation.Internal;

internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
{
    private readonly Func<TValue, TField> _getField;
    private readonly IList<IList<Delegate>> _chains = new List<IList<Delegate>>();

    public RuleContainer(
        Func<TValue, TField> getField
    )
    {
        _getField = getField;

        // call Then to init stage
        Then();
    }

    public IRuleBuilder<TValue, TField> When(Func<TField, bool> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    public IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, bool> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    public IRuleBuilder<TValue, TField> When(Func<TField, Task<bool>> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    public IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, Task<bool>> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    public IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate)
    {
        _chains[^1].Add(validate);

        return this;
    }

    public IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate)
    {
        _chains[^1].Add(validate);

        return this;
    }

    public IRuleBuilder<TValue, TField> Then()
    {
        _chains.Add(new List<Delegate>());

        return this;
    }

    public async Task<bool> ValidateAsync(
        ValidationContext<TValue> context,
        TValue value,
        int stage
    )
    {
        // no validation if no chain at this stage
        if (stage >= _chains.Count)
            return false;

        var field = _getField(value);

        foreach (var rule in _chains[stage])
            if (!await RunRuleAsync(context, field, rule))
                break;

        return true;
    }

    private async Task<bool> RunRuleAsync(ValidationContext<TValue> context, TField field, Delegate rule)
    {
        if (rule is Func<TField, bool> checkField)
            return checkField(field);

        if (rule is Func<ValidationContext<TValue>, TField, bool> checkContextField)
            return checkContextField(context, field);

        if (rule is Func<TField, Task<bool>> checkFieldAsync)
            return await checkFieldAsync(field);

        if (rule is Func<ValidationContext<TValue>, TField, Task<bool>> checkContextFieldAsync)
            return await checkContextFieldAsync(context, field);

        if (rule is Action<ValidationContext<TValue>, TField> validate)
            validate(context, field);

        if (rule is Func<ValidationContext<TValue>, TField, Task> validateAsync)
            await validateAsync(context, field);

        return true;
    }
}