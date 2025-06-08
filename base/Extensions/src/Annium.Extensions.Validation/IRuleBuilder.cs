using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation;

/// <summary>
/// Provides a fluent interface for building validation rules for a specific field within a value object.
/// Supports conditional validation, multi-stage execution, and both synchronous and asynchronous rules.
/// </summary>
/// <typeparam name="TValue">The type of the value being validated</typeparam>
/// <typeparam name="TField">The type of the field being validated</typeparam>
public interface IRuleBuilder<TValue, TField>
{
    /// <summary>
    /// Adds a conditional check that must pass for subsequent rules in the chain to execute
    /// </summary>
    /// <param name="check">Function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> When(Func<TField, bool> check);

    /// <summary>
    /// Adds a conditional check with access to validation context that must pass for subsequent rules to execute
    /// </summary>
    /// <param name="check">Function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, bool> check);

    /// <summary>
    /// Adds an asynchronous conditional check that must pass for subsequent rules in the chain to execute
    /// </summary>
    /// <param name="check">Async function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> When(Func<TField, Task<bool>> check);

    /// <summary>
    /// Adds an asynchronous conditional check with access to validation context that must pass for subsequent rules to execute
    /// </summary>
    /// <param name="check">Async function that returns true if subsequent rules should execute</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, Task<bool>> check);

    /// <summary>
    /// Adds a synchronous validation rule to the current chain
    /// </summary>
    /// <param name="validate">Validation action that can add errors to the context</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate);

    /// <summary>
    /// Adds an asynchronous validation rule to the current chain
    /// </summary>
    /// <param name="validate">Async validation function that can add errors to the context</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate);

    /// <summary>
    /// Starts a new validation stage/chain, allowing for multi-stage validation execution
    /// </summary>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> Then();
}
