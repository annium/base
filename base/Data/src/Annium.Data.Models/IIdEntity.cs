namespace Annium.Data.Models;

/// <summary>
/// Represents an entity with a unique identifier
/// </summary>
/// <typeparam name="TId">The type of the entity identifier</typeparam>
public interface IIdEntity<TId>
    where TId : struct
{
    /// <summary>
    /// Gets the unique identifier of the entity
    /// </summary>
    TId Id { get; }
}
