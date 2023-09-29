using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Composition.Internal;

internal interface ICompositionContainer<in TValue> where TValue : class
{
    IEnumerable<PropertyInfo> Fields { get; }

    Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label, ILocalizer localizer);
}