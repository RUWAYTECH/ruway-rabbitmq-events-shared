using MediatR;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se crea una tienda
/// </summary>
public record StoreCreatedEvent(
    Guid StoreId,
    string Code,
    string Name,
    string? Address,
    string? Email,
    Guid EnterpriseId
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(StoreCreatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de tiendas creadas
    /// </summary>
    public string RoutingKey => "memos.store.created";
}

/// <summary>
/// Evento que se publica cuando se actualiza una tienda
/// </summary>
public record StoreUpdatedEvent(
    Guid StoreId,
    string Code,
    string Name,
    string Address,
    string? Email,
    Guid EnterpriseId
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(StoreUpdatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de tiendas actualizadas
    /// </summary>
    public string RoutingKey => "memos.store.updated";
}

/// <summary>
/// Evento que se publica cuando se elimina una tienda
/// </summary>
public record StoreDeletedEvent(
    Guid StoreId
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(StoreDeletedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de tiendas eliminadas
    /// </summary>
    public string RoutingKey => "memos.store.deleted";
}