using System.Threading.Tasks;

namespace Annium.Extensions.Validation.Internal;

internal interface IRuleContainer<TValue>
{
    Task<bool> ValidateAsync(ValidationContext<TValue> context, TValue value, int stage);
}