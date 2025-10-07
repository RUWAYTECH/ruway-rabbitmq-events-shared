using MediatR;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se actualiza un empleado
/// </summary>
public record EmployeeUpdatedEvent(
    Guid EmployeeId,
    string Name,
    string LastName
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EmployeeUpdatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de empleados actualizados
    /// </summary>
    public string RoutingKey => "memos.employee.updated";
}