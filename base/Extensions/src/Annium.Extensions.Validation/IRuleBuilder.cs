using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation;

public interface IRuleBuilder<TValue, TField>
{
    IRuleBuilder<TValue, TField> When(Func<TField, bool> check);

    IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, bool> check);

    IRuleBuilder<TValue, TField> When(Func<TField, Task<bool>> check);

    IRuleBuilder<TValue, TField> When(Func<ValidationContext<TValue>, TField, Task<bool>> check);

    IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate);

    IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate);

    IRuleBuilder<TValue, TField> Then();
}