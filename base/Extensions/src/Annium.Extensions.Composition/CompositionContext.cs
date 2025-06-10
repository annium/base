using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Composition;

/// <summary>
/// Provides context information for field composition operations, including the root value, labels, and error reporting capabilities
/// </summary>
/// <typeparam name="TValue">The type of value being composed</typeparam>
public class CompositionContext<TValue>
{
    /// <summary>
    /// Gets the root value being composed
    /// </summary>
    public TValue Root { get; }

    /// <summary>
    /// Gets the label for the current composition context, used for error reporting
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the name of the field being composed
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// The result object for collecting errors during composition
    /// </summary>
    private readonly IResult _result;

    /// <summary>
    /// The localizer for translating error messages
    /// </summary>
    private readonly ILocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the CompositionContext class
    /// </summary>
    /// <param name="root">The root value being composed</param>
    /// <param name="label">The label for error reporting</param>
    /// <param name="field">The name of the field being composed</param>
    /// <param name="result">The result object for collecting errors</param>
    /// <param name="localizer">The localizer for translating error messages</param>
    internal CompositionContext(TValue root, string label, string field, IResult result, ILocalizer localizer)
    {
        Root = root;
        Label = label;
        Field = field;
        _result = result;
        _localizer = localizer;
    }

    /// <summary>
    /// Adds a localized error message to the composition result
    /// </summary>
    /// <param name="error">The error message key for localization</param>
    /// <param name="arguments">Arguments for the localized error message</param>
    public void Error(string error, params object[] arguments)
    {
        _result.Error(Label, _localizer[error, arguments]);
    }
}
