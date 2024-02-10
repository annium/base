using NodaTime;

namespace Annium.Data.Models;

public interface ICreatedUpdatedTimeEntity : ICreatedTimeEntity
{
    Instant UpdatedAt { get; }
}
