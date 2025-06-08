using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Composition;

/// <summary>
/// Interface for building composition rules for a specific field
/// </summary>
/// <typeparam name="TValue">The type of value being composed</typeparam>
/// <typeparam name="TField">The type of field being configured</typeparam>
public interface IRuleBuilder<TValue, TField>
{
    /// <summary>
    /// Adds a synchronous condition to the rule
    /// </summary>
    /// <param name="check">The condition function</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, bool> check);

    /// <summary>
    /// Adds an asynchronous condition to the rule
    /// </summary>
    /// <param name="check">The condition function</param>
    /// <returns>The rule builder for method chaining</returns>
    IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, Task<bool>> check);

    /// <summary>
    /// Sets the synchronous loading function for the field
    /// </summary>
    /// <param name="load">The loading function</param>
    /// <param name="message">Optional error message</param>
    void LoadWith(Func<CompositionContext<TValue>, TField?> load, string message = "");

    /// <summary>
    /// Sets the asynchronous loading function for the field
    /// </summary>
    /// <param name="load">The loading function</param>
    /// <param name="message">Optional error message</param>
    void LoadWith(Func<CompositionContext<TValue>, Task<TField?>> load, string message = "");
}
