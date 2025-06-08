using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Composition.Internal;

/// <summary>
/// Internal interface for composition containers that handle field-specific composition logic for a value type
/// </summary>
/// <typeparam name="TValue">The type of value to compose</typeparam>
internal interface ICompositionContainer<in TValue>
    where TValue : class
{
    /// <summary>
    /// Gets the collection of fields that this container can compose
    /// </summary>
    IEnumerable<PropertyInfo> Fields { get; }

    /// <summary>
    /// Composes the specified value by applying the container's field rules
    /// </summary>
    /// <param name="value">The value to compose</param>
    /// <param name="label">The label for error reporting context</param>
    /// <param name="localizer">The localizer for translating error messages</param>
    /// <returns>A status result indicating the success or failure of the composition operation</returns>
    Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label, ILocalizer localizer);
}
