using MediatR;
using Ruway.Events.Command.Interfaces.Constants;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se elimina un empleado
/// </summary>
public record EmployeeDeletedEvent(
    Guid EmployeeId
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EmployeeDeletedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de empleados eliminados
    /// </summary>
    public string RoutingKey => EventConstants.EmployeeEvents.EmployeeDeleted;
}