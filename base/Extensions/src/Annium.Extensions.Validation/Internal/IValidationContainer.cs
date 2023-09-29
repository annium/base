using System;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Validation.Internal;

internal interface IValidationContainer<in TValue>
{
    Task<ValueTuple<IResult, bool>> ValidateAsync(TValue value, string label, int stage, ILocalizer localizer);
}