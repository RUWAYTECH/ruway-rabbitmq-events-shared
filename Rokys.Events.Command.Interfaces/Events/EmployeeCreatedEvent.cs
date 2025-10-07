using MediatR;

namespace Rokys.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se crea un empleado
/// </summary>
public record EmployeeCreatedEvent(
    Guid EmployeeId,
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Email,
    string Phone
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EmployeeCreatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de empleados creados
    /// </summary>
    public string RoutingKey => "memos.employee.created";
}