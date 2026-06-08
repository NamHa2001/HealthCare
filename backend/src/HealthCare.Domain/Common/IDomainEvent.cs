using MediatR;

namespace HealthCare.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid EventId => Guid.NewGuid();
    DateTime OccurredOn => DateTime.UtcNow;
}
