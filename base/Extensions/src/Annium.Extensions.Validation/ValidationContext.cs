using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Validation;

/// <summary>
/// Provides context information and error reporting capabilities during validation
/// </summary>
/// <typeparam name="TValue">The type of the root object being validated</typeparam>
public class ValidationContext<TValue>
{
    /// <summary>
    /// Gets the root object being validated
    /// </summary>
    public TValue Root { get; }

    /// <summary>
    /// Gets the label for the current field being validated
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the name of the current field being validated
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// The result object for collecting validation errors
    /// </summary>
    private readonly IResult _result;

    /// <summary>
    /// The localizer for translating error messages
    /// </summary>
    private readonly ILocalizer _localizer;

    internal ValidationContext(TValue root, string label, string field, IResult result, ILocalizer localizer)
    {
        Root = root;
        Label = label;
        Field = field;
        _result = result;
        _localizer = localizer;
    }

    /// <summary>
    /// Reports a validation error for the current field
    /// </summary>
    /// <param name="error">The error message or localization key</param>
    /// <param name="arguments">Arguments for string formatting or localization</param>
    public void Error(string error, params object[] arguments)
    {
        _result.Error(Label, _localizer[error, arguments]);
    }
}
