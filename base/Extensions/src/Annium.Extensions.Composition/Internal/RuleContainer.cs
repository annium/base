using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Extensions.Composition.Internal;

/// <summary>
/// Internal implementation of a rule container that handles field composition with conditional loading and validation
/// </summary>
/// <typeparam name="TValue">The type of value being composed</typeparam>
/// <typeparam name="TField">The type of field being configured</typeparam>
internal class RuleContainer<TValue, TField> : IRuleBuilder<TValue, TField>, IRuleContainer<TValue>
{
    /// <summary>
    /// The setter method for the target field
    /// </summary>
    private readonly MethodInfo _setTarget;

    /// <summary>
    /// The conditional check function for determining when to apply the rule
    /// </summary>
    private Delegate? _check;

    /// <summary>
    /// The loading function for retrieving the field value
    /// </summary>
    private Delegate? _load;

    /// <summary>
    /// The error message to display when loading fails
    /// </summary>
    private string _message = string.Empty;

    /// <summary>
    /// Whether to allow default values for the field
    /// </summary>
    private readonly bool _allowDefault;

    /// <summary>
    /// Initializes a new instance of the RuleContainer class
    /// </summary>
    /// <param name="setTarget">The setter method for the target field</param>
    /// <param name="allowDefault">Whether to allow default values for the field</param>
    public RuleContainer(MethodInfo setTarget, bool allowDefault)
    {
        _setTarget = setTarget;
        _allowDefault = allowDefault;
    }

    /// <summary>
    /// Adds a synchronous condition to the rule
    /// </summary>
    /// <param name="check">The condition function</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, bool> check)
    {
        _check = check;

        return this;
    }

    /// <summary>
    /// Adds an asynchronous condition to the rule
    /// </summary>
    /// <param name="check">The condition function</param>
    /// <returns>The rule builder for method chaining</returns>
    public IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, Task<bool>> check)
    {
        _check = check;

        return this;
    }

    /// <summary>
    /// Sets the synchronous loading function for the field
    /// </summary>
    /// <param name="load">The loading function</param>
    /// <param name="message">Optional error message when loading fails</param>
    public void LoadWith(Func<CompositionContext<TValue>, TField?> load, string message = "")
    {
        _load = load;
        _message = message;
    }

    /// <summary>
    /// Sets the asynchronous loading function for the field
    /// </summary>
    /// <param name="load">The loading function</param>
    /// <param name="message">Optional error message when loading fails</param>
    public void LoadWith(Func<CompositionContext<TValue>, Task<TField?>> load, string message = "")
    {
        _load = load;
        _message = message;
    }

    /// <summary>
    /// Executes the rule's composition logic for the field
    /// </summary>
    /// <param name="context">The composition context containing field information and error reporting</param>
    /// <param name="value">The value being composed</param>
    /// <returns>A task representing the asynchronous composition operation</returns>
    public async Task ComposeAsync(CompositionContext<TValue> context, TValue value)
    {
        if (!await CheckAsync(context))
            return;

        var target = await LoadAsync(context);

        if ((target is null || target.Equals(default(TField)!)) && !_allowDefault)
            context.Error(string.IsNullOrEmpty(_message) ? "{0} not found" : _message, context.Field);
        else
            _setTarget.Invoke(value, new object[] { target! });
    }

    /// <summary>
    /// Evaluates the conditional check if one is configured
    /// </summary>
    /// <param name="context">The composition context</param>
    /// <returns>True if the condition passes or no condition is configured, false otherwise</returns>
    private async Task<bool> CheckAsync(CompositionContext<TValue> context) =>
        _check switch
        {
            Func<CompositionContext<TValue>, bool> check => check(context),
            Func<CompositionContext<TValue>, Task<bool>> check => await check(context),
            _ => true,
        };

    /// <summary>
    /// Executes the loading function to retrieve the field value
    /// </summary>
    /// <param name="context">The composition context</param>
    /// <returns>The loaded field value</returns>
    /// <exception cref="InvalidOperationException">Thrown when no loading function is configured</exception>
    private async Task<TField?> LoadAsync(CompositionContext<TValue> context) =>
        _load switch
        {
            Func<CompositionContext<TValue>, TField?> load => load(context),
            Func<CompositionContext<TValue>, Task<TField?>> load => await load(context),
            _ => throw new InvalidOperationException($"{context.Field} has no {nameof(LoadWith)} defined."),
        };
}
