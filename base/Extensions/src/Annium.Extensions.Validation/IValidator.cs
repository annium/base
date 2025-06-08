using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation;

/// <summary>
/// Interface for validating values using configured rules
/// </summary>
/// <typeparam name="TValue">The type of value to validate</typeparam>
public interface IValidator<TValue>
{
    /// <summary>
    /// Validates the specified value asynchronously
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="label">The label for the validation context</param>
    /// <returns>The validation result</returns>
    Task<IResult> ValidateAsync(TValue value, string label = "");
}
