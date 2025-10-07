using MediatR;

namespace Rokys.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se crea una empresa
/// </summary>
public record EnterpriseCreatedEvent(
    Guid EnterpriseId,
    string Name,
    string BusinessName,
    string DocumentNumber
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EnterpriseCreatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de empresas creadas
    /// </summary>
    public string RoutingKey => "memos.enterprise.created";
}

/// <summary>
/// Evento que se publica cuando se actualiza una empresa
/// </summary>
public record EnterpriseUpdatedEvent(
    Guid EnterpriseId,
    string Name,
    string BusinessName,
    string DocumentNumber
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EnterpriseUpdatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de empresas actualizadas
    /// </summary>
    public string RoutingKey => "memos.enterprise.updated";
}

/// <summary>
/// Evento que se publica cuando se elimina una empresa
/// </summary>
public record EnterpriseDeletedEvent(
    Guid EnterpriseId
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(EnterpriseDeletedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de empresas eliminadas
    /// </summary>
    public string RoutingKey => "memos.enterprise.deleted";
}