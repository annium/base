using System;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Validation.Internal;

/// <summary>
/// Internal interface for validation containers that execute validation rules against values
/// </summary>
/// <typeparam name="TValue">The type of the value being validated</typeparam>
internal interface IValidationContainer<in TValue>
{
    /// <summary>
    /// Validates the specified value using all configured rules for the given stage
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="label">The label to use for error messages (used for nested property paths)</param>
    /// <param name="stage">The validation stage to execute</param>
    /// <param name="localizer">The localizer for translating error messages</param>
    /// <returns>A tuple containing the validation result and whether any rules were executed for this stage</returns>
    Task<ValueTuple<IResult, bool>> ValidateAsync(TValue value, string label, int stage, ILocalizer localizer);
}
