using NodaTime;

namespace Annium.Data.Models;

public interface ICreatedTimeEntity
{
    Instant CreatedAt { get; }
}
