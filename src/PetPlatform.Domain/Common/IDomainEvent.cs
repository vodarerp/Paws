namespace PetPlatform.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
