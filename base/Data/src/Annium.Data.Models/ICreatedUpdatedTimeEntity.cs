using NodaTime;

namespace Annium.Data.Models;

/// <summary>
/// Represents an entity that tracks both creation and update times
/// </summary>
public interface ICreatedUpdatedTimeEntity : ICreatedTimeEntity
{
    /// <summary>
    /// Gets the time when the entity was last updated
    /// </summary>
    Instant UpdatedAt { get; }
}
