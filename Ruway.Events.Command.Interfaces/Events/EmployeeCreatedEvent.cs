using MediatR;
using Ruway.Events.Command.Interfaces.Constants;

namespace Ruway.Events.Command.Interfaces.Events;

public record EmployeeStores(
   Guid StoreId,
   DateTime AssignmentDate
)
{

}

/// <summary>
/// Evento que se publica cuando se crea un empleado
/// </summary>
public record EmployeeCreatedEvent(
    Guid EmployeeId,
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Email,
    string Phone,
    string PersonalEmail,
    EmployeeStores[] EmployeeStores
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EmployeeCreatedEvent);
    public int Version => 1;

    /// <summary>
    /// Routing key específica para eventos de empleados creados
    /// </summary>
    public string RoutingKey => EventConstants.EmployeeEvents.EmployeeCreated;
}