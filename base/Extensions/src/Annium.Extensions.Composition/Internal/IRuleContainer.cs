using System.Threading.Tasks;

namespace Annium.Extensions.Composition.Internal;

/// <summary>
/// Internal interface for rule containers that execute composition logic for individual fields
/// </summary>
/// <typeparam name="TValue">The type of value being composed</typeparam>
internal interface IRuleContainer<TValue>
{
    /// <summary>
    /// Executes the rule's composition logic for a specific field
    /// </summary>
    /// <param name="context">The composition context containing field information and error reporting</param>
    /// <param name="value">The value being composed</param>
    /// <returns>A task representing the asynchronous composition operation</returns>
    Task ComposeAsync(CompositionContext<TValue> context, TValue value);
}
