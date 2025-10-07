using MediatR;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se crea un usuario
/// </summary>
public record UserCreatedEvent(
    Guid UserId,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string[]? Roles = null
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserCreatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de usuarios creados
    /// </summary>
    public string RoutingKey => "security.user.created";
}

/// <summary>
/// Evento que se publica cuando se actualiza un usuario
/// </summary>
public record UserUpdatedEvent(
    Guid UserId,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string[]? Roles = null
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserUpdatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de usuarios actualizados
    /// </summary>
    public string RoutingKey => "security.user.updated";
}

/// <summary>
/// Evento que se publica cuando se elimina un usuario
/// </summary>
public record UserDeletedEvent(
    Guid UserId,
    string UserName
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserDeletedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de usuarios eliminados
    /// </summary>
    public string RoutingKey => "security.user.deleted";
}

/// <summary>
/// Evento que se publica cuando un usuario se autentica
/// </summary>
public record UserLoggedInEvent(
    Guid UserId,
    string UserName,
    string IpAddress,
    string UserAgent
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserLoggedInEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de login de usuarios
    /// </summary>
    public string RoutingKey => "security.user.logged_in";
}

/// <summary>
/// Evento que se publica cuando un usuario cierra sesión
/// </summary>
public record UserLoggedOutEvent(
    Guid UserId,
    string UserName
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserLoggedOutEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de logout de usuarios
    /// </summary>
    public string RoutingKey => "security.user.logged_out";
}