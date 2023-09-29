using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Composition;

public interface IRuleBuilder<TValue, TField>
{
    IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, bool> check);

    IRuleBuilder<TValue, TField> When(Func<CompositionContext<TValue>, Task<bool>> check);

    void LoadWith(Func<CompositionContext<TValue>, TField?> load, string message = "");

    void LoadWith(Func<CompositionContext<TValue>, Task<TField?>> load, string message = "");
}