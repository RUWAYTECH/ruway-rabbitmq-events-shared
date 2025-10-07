using MediatR;

namespace Rokys.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se asigna un empleado a una tienda
/// </summary>
public record EmployeeStoreAssignedEvent(
    Guid EmployeeId,
    Guid StoreId,
    DateTime AssignedDate
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EmployeeStoreAssignedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de asignación empleado-tienda
    /// </summary>
    public string RoutingKey => "memos.employee_store.assigned";
}

/// <summary>
/// Evento que se publica cuando se desasigna un empleado de una tienda
/// </summary>
public record EmployeeStoreUnassignedEvent(
    Guid EmployeeId,
    Guid StoreId,
    DateTime UnassignedDate,
    string? Reason = null
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EmployeeStoreUnassignedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de desasignación empleado-tienda
    /// </summary>
    public string RoutingKey => "memos.employee_store.unassigned";
}