using MediatR;
using Ruway.Events.Command.Interfaces.Constants;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se crea una persona
/// </summary>
public record PeopleCreatedEvent(
    Guid UserReferenceId,
    Guid? EmployeeId,
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Email,
    string PersonalEmail,
    string Phone,
    string RoleCode,
    bool IsExternal,
    bool IsActive
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(PeopleCreatedEvent);
    public int Version => 1;

    /// <summary>
    /// Routing key específica para eventos de personas creadas
    /// </summary>
    public string RoutingKey => EventConstants.PeopleEvents.PeopleCreated;
}