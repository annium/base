using System.Threading.Tasks;

namespace Annium.Extensions.Composition.Internal;

internal interface IRuleContainer<TValue>
{
    Task ComposeAsync(CompositionContext<TValue> context, TValue value);
}