using MediatR;
using Ruway.Events.Command.Interfaces.Constants;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se actualiza una persona
/// </summary>
public record PeopleDeletedEvent(
    Guid UserId
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(PeopleDeletedEvent);
    public int Version => 1;

    /// <summary>
    /// Routing key específica para eventos de personas actualizadas
    /// </summary>
    public string RoutingKey => EventConstants.PeopleEvents.PeopleDeleted;
}