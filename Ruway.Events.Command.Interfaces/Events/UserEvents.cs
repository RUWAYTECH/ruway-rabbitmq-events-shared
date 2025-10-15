using MediatR;
using Ruway.Events.Command.Interfaces.Constants;
using Ruway.Events.Command.Interfaces.Enums;

namespace Ruway.Events.Command.Interfaces.Events;

/// <summary>
/// Evento que se publica cuando se crea un usuario
/// </summary>
public record UserCreatedEvent(
    Guid UserId,
    Guid EmployeeId,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string ApplicationCode,
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
    public string RoutingKey =>  EventConstants.UserEvents.UserCreated;
}

/// <summary>
/// Evento que se publica cuando se actualiza un usuario
/// </summary>
public record UserUpdatedEvent(
    Guid UserId,
    Guid? EmployeeId,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string ApplicationCode,
    string RoleCodes = null,
    string RoleNames = null
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserUpdatedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de usuarios actualizados
    /// </summary>
    public string RoutingKey => EventConstants.UserEvents.UserUpdated;
}

/// <summary>
/// Evento que se publica cuando se elimina un usuario
/// </summary>
public record UserDeletedEvent(
    Guid UserId,
    string ApplicationCode
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserDeletedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de usuarios eliminados
    /// </summary>
    public string RoutingKey => EventConstants.UserEvents.UserDeleted;
}




public record UserRoleAssignedEvent(
    Guid UserId,
    string RoleCode,
    string RoleName,
    string ApplicationCode,
    UserActions Actions = UserActions.All
) : IDomainEvent, INotification, IRoutableEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventName => nameof(UserRoleAssignedEvent);
    public int Version => 1;
    
    /// <summary>
    /// Routing key específica para eventos de usuarios eliminados
    /// </summary>
    public string RoutingKey => EventConstants.UserEvents.UserRoleAssigned;
}