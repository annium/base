namespace Annium.Net.Types.Refs;

/// <summary>
/// Interface for type references that point to named type models.
/// Provides namespace and name identification for the referenced model.
/// </summary>
public interface IModelRef : IRef
{
    /// <summary>
    /// The namespace of the referenced model.
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// The name of the referenced model.
    /// </summary>
    string Name { get; }
}
