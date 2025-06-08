using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Extensions.Composition;

/// <summary>
/// Interface for composing values using rules
/// </summary>
/// <typeparam name="TValue">The type of value to compose</typeparam>
public interface IComposer<TValue>
    where TValue : class
{
    /// <summary>
    /// Composes the specified value by applying configured rules
    /// </summary>
    /// <param name="value">The value to compose</param>
    /// <param name="label">The label for the composition context</param>
    /// <returns>The result of the composition operation</returns>
    Task<IStatusResult<OperationStatus>> ComposeAsync(TValue value, string label = "");
}
