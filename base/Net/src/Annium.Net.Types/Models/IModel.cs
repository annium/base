namespace Annium.Net.Types.Models;

/// <summary>
/// Base interface for all type models representing mapped .NET types.
/// Provides fundamental identification properties for namespace and name.
/// </summary>
public interface IModel
{
    /// <summary>
    /// The namespace containing this type model.
    /// </summary>
    Namespace Namespace { get; }

    /// <summary>
    /// The name of this type model.
    /// </summary>
    string Name { get; }
}
