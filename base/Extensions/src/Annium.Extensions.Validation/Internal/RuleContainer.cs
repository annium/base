using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation.Internal;

/// <summary>
/// Internal implementation of a rule container that manages validation rules for a specific field within a value object.
/// Supports conditional validation, multi-stage execution, and fluent rule building.
/// </summary>
/// <typeparam name="TValue">The type of the value being validated</typeparam>
/// <typeparam name="TField">The type of the field being validated</typeparam>
internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
{
    /// <summary>
    /// Function to extract the field value from the root value object
    /// </summary>
    private readonly Func<TValue, TField> _getField;

    /// <summary>
    /// Collection of validation chains, where each chain represents a validation stage
    /// </summary>
    private readonly IList<IList<Delegate>> _chains = new List<IList<Delegate>>();

    /// <summary>
    /// Initializes a new rule container for the specified field
    /// </summary>
    /// <param name="getField">Function to extract the field value from the root value object</param>
    public RuleContainer(Func<TValue, TField> getField)
    {
        _getField = getField;

        // call Then to init stage
        Then();
    }

    /// <summary>
    /// Adds a conditional check that must pass for subsequent rules in the chain to execute
    /// </summary>
    /// <param name="check">Function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> When(Func<TField, bool> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    /// <summary>
    /// Adds a conditional check with access to validation context that must pass for subsequent rules to execute
    /// </summary>
    /// <param name="check">Function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, bool> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    /// <summary>
    /// Adds an asynchronous conditional check that must pass for subsequent rules in the chain to execute
    /// </summary>
    /// <param name="check">Async function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> When(Func<TField, Task<bool>> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    /// <summary>
    /// Adds an asynchronous conditional check with access to validation context that must pass for subsequent rules to execute
    /// </summary>
    /// <param name="check">Async function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, Task<bool>> check)
    {
        _chains[^1].Add(check);

        return this;
    }

    /// <summary>
    /// Adds a synchronous validation rule to the current chain
    /// </summary>
    /// <param name="validate">Validation action that can add errors to the context</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate)
    {
        _chains[^1].Add(validate);

        return this;
    }

    /// <summary>
    /// Adds an asynchronous validation rule to the current chain
    /// </summary>
    /// <param name="validate">Async validation function that can add errors to the context</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate)
    {
        _chains[^1].Add(validate);

        return this;
    }

    /// <summary>
    /// Starts a new validation stage/chain, allowing for multi-stage validation execution
    /// </summary>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> Then()
    {
        _chains.Add(new List<Delegate>());

        return this;
    }

    /// <summary>
    /// Validates the field value against all rules in the specified stage
    /// </summary>
    /// <param name="context">The validation context for error reporting</param>
    /// <param name="value">The root value object</param>
    /// <param name="stage">The validation stage to execute</param>
    /// <returns>True if the stage was executed, false if no rules exist for this stage</returns>
    public async Task<bool> ValidateAsync(ValidationContext<TValue> context, TValue value, int stage)
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

    /// <summary>
    /// Executes a single rule delegate, handling different rule types (conditions and validations)
    /// </summary>
    /// <param name="context">The validation context</param>
    /// <param name="field">The field value to validate</param>
    /// <param name="rule">The rule delegate to execute</param>
    /// <returns>True if validation should continue, false if a condition failed and the chain should stop</returns>
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
