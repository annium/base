using System.Threading.Tasks;

namespace Annium.Extensions.Validation.Internal;

/// <summary>
/// Internal interface for rule containers that handle validation of specific fields within a value object
/// </summary>
/// <typeparam name="TValue">The type of the value being validated</typeparam>
internal interface IRuleContainer<TValue>
{
    /// <summary>
    /// Validates the specified value against the rules defined in this container for a given stage
    /// </summary>
    /// <param name="context">The validation context containing result handling and localization</param>
    /// <param name="value">The value to validate</param>
    /// <param name="stage">The validation stage to execute (supports multi-stage validation)</param>
    /// <returns>True if the stage was executed (even if validation failed), false if no rules exist for this stage</returns>
    Task<bool> ValidateAsync(ValidationContext<TValue> context, TValue value, int stage);
}
