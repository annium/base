namespace Annium.Net.Types.Refs;

/// <summary>
/// Interface for type references that point to generic type models.
/// Extends IModelRef with generic type argument information.
/// </summary>
public interface IGenericModelRef : IModelRef
{
    /// <summary>
    /// The generic type arguments for the referenced model.
    /// </summary>
    IRef[] Args { get; }
}
