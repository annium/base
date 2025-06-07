using NodaTime;

namespace Annium.Data.Models;

/// <summary>
/// Represents an entity that tracks its creation time
/// </summary>
public interface ICreatedTimeEntity
{
    /// <summary>
    /// Gets the time when the entity was created
    /// </summary>
    Instant CreatedAt { get; }
}
